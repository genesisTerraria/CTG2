sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float2 uTargetPosition;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

// This is a shader. You are on your own with shaders. Compile shaders in an XNB project.

float4 PixelShaderFunction(float4 baseCol : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	float2 rel = float2(0.5 - coords.x, 0.5 - coords.y);

	float2 dir = uTargetPosition;
	float2 perp = float2(dir.y, -dir.x);
	float d = dot(rel, dir);
	float d2 = dot(rel, perp);
	rel = dir * d * 4 + perp * d2;

	float dist = length(rel) * 1.8;
	float p = abs(uOpacity - dist);

	float size = 0.15 + 0.15 * (uOpacity);
	if(p < size){
		p = 1 - p/size;
		return baseCol / baseCol.a * p * p * (1 - uOpacity*uOpacity) * uSaturation;
	}

	return float4(0, 0, 0, 0);
}

technique Technique1
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}