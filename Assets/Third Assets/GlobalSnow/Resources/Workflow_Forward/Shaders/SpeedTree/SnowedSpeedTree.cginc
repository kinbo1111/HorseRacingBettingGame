	
	uniform float4    _GS_SnowData1;	// x = relief, y = occlusion, z = glitter, w = brightness
	uniform float4    _GS_SnowData2;	// x = minimum altitude, y = altitude scattering, z = coverage extension
    uniform float4    _GS_SnowCamPos;
    uniform half4     _GS_SnowTint;
	uniform sampler2D_float _GS_DepthTexture;
    
	// get snow coverage on trees
	void SetTreeCoverage(Input IN, inout SurfaceOutput o) { 

		// prevent snow on sides and below minimum altitude
		float3 wsNormal = IN.worldNormal; 
		float ny = wsNormal.y - 0.2;
		float flatSurface = saturate(ny * 10.0);
		float minAltitude = saturate( IN.worldPos.y - _GS_SnowData2.x);

		fixed snowCover   = minAltitude * flatSurface;

          // mask support
        #if defined(GLOBALSNOW_NATURE_MASK)
	        float2 maskUV = (IN.worldPos.xz - _GS_DepthMaskWorldSize.yw) / _GS_DepthMaskWorldSize.xz + 0.5.xx;
		    fixed mask = tex2D(_GS_DepthMask, maskUV).a;
			if (any(floor(maskUV)!=0)) mask = 1;
			fixed *= mask;
        #endif


		
		// pass color data to output shader
		o.Albedo = _GS_SnowData1.www;
        o.Albedo.rgb *= _GS_SnowTint.rgb;
		o.Alpha *= snowCover;
	}	
