	float4    _GS_SnowData1;	// x = relief, y = occlusion, z = glitter, w = brightness
	float4    _GS_SnowData2;	// x = minimum altitude, y = altitude scattering, z = coverage extension
	float4    _GS_SnowData3;	// x = Sun occlusion, y = sun atten, z = ground coverage, w = grass coverage
    float4    _GS_SnowCamPos;
    half4     _GS_SnowTint;
    sampler2D_float _GS_DepthTexture;
	
	sampler2D _GS_DepthMask;
	float4 _GS_DepthMaskWorldSize;

struct Input {
	float2 uv_MainTex;
	fixed4 color : COLOR;
	float3 worldPos;
};


	// get snow coverage on grass
	void SetGrassCoverage(Input IN, inout SurfaceOutput o) { 
		// prevent snow on sides and below minimum altitude
		float minAltitude = saturate( IN.worldPos.y - _GS_SnowData2.x);
		float snowCover   = minAltitude * saturate(IN.uv_MainTex.y + _GS_SnowData3.w);

        // mask support
        #if defined(GLOBALSNOW_NATURE_MASK)
	        float2 maskUV = (IN.worldPos.xz - _GS_DepthMaskWorldSize.yw) / _GS_DepthMaskWorldSize.xz + 0.5.xx;
		    fixed mask = tex2D(_GS_DepthMask, maskUV).a;
			if (any(floor(maskUV)!=0)) mask = 1;
			snowCover *= mask;
		#endif

		// pass color data to output shader
		o.Albedo = _GS_SnowData1.www;
        o.Albedo.rgb *= _GS_SnowTint.rgb;
		o.Alpha = snowCover * 0.96;
	}
	
	
	
