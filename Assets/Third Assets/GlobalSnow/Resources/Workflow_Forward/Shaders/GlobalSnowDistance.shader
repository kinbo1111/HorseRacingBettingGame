Shader "GlobalSnow/SnowDistance" {
	Properties {
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_Color ("Tint", Vector) = (0.8,0.8,1,1)
	}
        	
	CGINCLUDE
	#pragma fragmentoption ARB_precision_hint_fastest
	#pragma multi_compile __ NORMALS_GBUFFER IGNORE_NORMALS
	#pragma multi_compile __ IGNORE_COVERAGE
	#pragma target 3.0
	#include "UnityCG.cginc"

	#define SNOW_NORMAL_THRESHOLD 0.6
	
	sampler2D		_MainTex;	// frame buffer
	float4			_MainTex_TexelSize;
	float4			_MainTex_ST;
	sampler2D_float	_CameraDepthTexture;		// depth from view
    sampler2D_float _GS_DepthTexture;

    #if !IGNORE_NORMALS
	#if NORMALS_GBUFFER
	sampler2D 		_CameraGBufferTexture2;	// for deferred normals
	#else
	sampler2D_float _CameraDepthNormalsTexture;
	#endif
	#endif

    float4x4 		_ClipToWorld;
    float4x4        _CamToWorld;
    float4 			_GS_SnowCamPos;
	float4			_GS_SunDir;
	
  	sampler2D _GS_SnowTex;
	float4    _GS_SnowData1;	// x = relief, y = occlusion, z = glitter, w = brightness
	float4    _GS_SnowData2;	// x = minimum altitude, y = altitude scattering, z = coverage extension
	float4    _GS_SnowData3;
	float4    _GS_SnowData5;    // x = slope threshold, y = slope sharpness, z = slope noise
	float3    _GS_SnowData6;    // x = slope threshold, y = slope sharpness, z = slope noise, w = distance slope threshold
	float     _Distance01;
	fixed4    _Color;
    float     _DistanceSlopeThreshold;
	
	sampler2D _GS_DepthMask;
	float4 _GS_DepthMaskWorldSize;

    struct appdata {
    	float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
    };
    
	struct v2f {
	    float4 pos : SV_POSITION;
	    float2 uv: TEXCOORD0;
    	float2 depthUV : TEXCOORD1;
    	float3 cameraToFarPlane : TEXCOORD2;
	};

	v2f vert(appdata v) {
    	v2f o;
    	o.pos = UnityObjectToClipPos(v.vertex);
   		o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
    	o.depthUV = o.uv;
   	      
    	#if UNITY_UV_STARTS_AT_TOP
    	if (_MainTex_TexelSize.y < 0) {
	        // Depth texture is inverted WRT the main texture
    	    o.depthUV.y = 1 - o.depthUV.y;
    	}
    	#endif
               
    	// Clip space X and Y coords
    	float2 clipXY = o.pos.xy / o.pos.w;
               
    	// Position of the far plane in clip space
    	float4 farPlaneClip = float4(clipXY, 1, 1);
               
    	// Homogeneous world position on the far plane
    	farPlaneClip.y *= _ProjectionParams.x;	
   		_ClipToWorld = mul(_ClipToWorld, unity_CameraInvProjection);
    	float4 farPlaneWorld4 = mul(_ClipToWorld, farPlaneClip);
               
    	// World position on the far plane
    	float3 farPlaneWorld = farPlaneWorld4.xyz / farPlaneWorld4.w;
               
    	// Vector from the camera to the far plane
    	o.cameraToFarPlane = farPlaneWorld - _WorldSpaceCameraPos;
    	
    	return o;
	}


	float3 getWorldPos(v2f i, float depth01) {
		return i.cameraToFarPlane * depth01 + _WorldSpaceCameraPos;
    }


	inline fixed3 LambertLight (float3 wsNormal, float3 albedo) {
		fixed diff = max (0, dot (wsNormal, -_GS_SunDir.xyz));
		return albedo * diff;
	}

	// get snow coverage
	fixed4 frag (v2f i) : SV_Target {
		// discard sky
		float depth01 = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV)));
		if (depth01<_Distance01 || depth01>0.999) return 0;	

		// pixel world position
		float3 worldPos = getWorldPos(i, depth01);

		// get snow coverage
		#if IGNORE_COVERAGE
			float snowCover = 1.0;
		#else
	        float4 st = float4(worldPos.xz - _GS_SnowCamPos.xz, 0, 0);
			st = (st * _GS_SnowData2.z) + 0.5;
			float zmask = tex2Dlod(_GS_DepthTexture, st).x;
			if (any(floor(st)!=0)) zmask = 1.0;
			float y = max(_GS_SnowCamPos.y - worldPos.y, 0.001) / _GS_SnowCamPos.w;
			float zd = min(zmask + _GS_SnowData3.z, y);
			float snowCover = saturate( ((zd / y) - 0.9875) * 110);

			#if !defined(IGNORE_COVERAGE_MASK)
				float2 maskUV = (worldPos.xz - _GS_DepthMaskWorldSize.yw) / _GS_DepthMaskWorldSize.xz + 0.5.xx;
				fixed mask = tex2D(_GS_DepthMask, maskUV).a;
				if (any(floor(maskUV)!=0)) mask = 1;
				snowCover *= mask;
			#endif
		#endif

		// prevent snow on walls and below minimum altitude
	    #if !IGNORE_NORMALS
			#if NORMALS_GBUFFER
				float3 wsNormal = tex2D(_CameraGBufferTexture2, i.uv).xyz * 2.0 - 1.0;	// for deferred
			#else
				float3 wsNormal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.depthUV));
 				wsNormal = mul((float3x3)_CamToWorld, wsNormal);
			#endif
		#endif

		fixed3 diff = tex2D(_GS_SnowTex, worldPos.xz * 0.02).rgb;

		// prevent snow on walls and below minimum altitude
		float altG = tex2D(_GS_SnowTex, worldPos.xz * 0.5).g;
		float altNoise = diff.r * altG - 0.9;
		#if !IGNORE_NORMALS
			#if !SHADER_API_D3D9
				float ny = wsNormal.y - _DistanceSlopeThreshold;
				float flatSurface = saturate( (ny + altNoise * _GS_SnowData5.z) * _GS_SnowData5.y);
			#else
				float ny = wsNormal.y - 0.7;
				float flatSurface = saturate(ny * 15.0);
			#endif
		#else
			const float flatSurface = 1.0;
		#endif
		float minAltitude = worldPos.y - _GS_SnowData2.w - altNoise * _GS_SnowData2.y;
		minAltitude = saturate(minAltitude / _GS_SnowData6.z);
		snowCover = snowCover * minAltitude * flatSurface - _GS_SnowData6.x;
		if (snowCover <=0) {	
			return fixed4(0,0,0,0);
		}

		snowCover = saturate(snowCover);
		snowCover*= saturate((depth01 - _Distance01) / (_Distance01 * 0.111));

		// Final color
		diff *= _Color.rgb;
		#if IGNORE_NORMALS
			fixed3 color = lerp(diff , diff * _GS_SnowData1.www, saturate(0.65 + _GS_SunDir.y));
		#else
			fixed3 color = lerp(diff , LambertLight(wsNormal * _GS_SnowData1.www, diff), saturate(0.65 + _GS_SunDir.y));
		#endif
		return fixed4(color, snowCover);
	}

	ENDCG
	
	SubShader {
       	ZTest Always Cull Off ZWrite Off
       	Fog { Mode Off }
		Pass {
	        CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
        }
	}
	FallBack Off
}	