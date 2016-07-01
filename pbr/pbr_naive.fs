#define PI										3.14159265358979323846
#define PI_INV									0.31830988618379067154

in vec2 Texcoord;
in vec3 Position;
in vec3 Normal;

out vec3 image_out;

uniform vec4 diffuse_color;
uniform sampler2D diffuse_texture;					

uniform vec4 specular_color;
uniform sampler2D specular_texture;

uniform vec4 ambient_color;
uniform sampler2D ambient_texture;

uniform samplerCube			env_map;
uniform samplerCube			env_map_filtered;
uniform vec3 				eyepos;


 float radicalInverse_VdC(uint bits) {
     bits = (bits << 16u) | (bits >> 16u);
     bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
     bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
     bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
     bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
     return float(bits) * 2.3283064365386963e-10; // / 0x100000000
 }
 vec2 Hammersley(uint i, uint N) {
     return vec2(float(i)/float(N), radicalInverse_VdC(i));
 }

vec3 fresnel_schlick(vec3 f0, float VdotH) {
	return f0 + (1.0 - f0) * pow(1.0 - VdotH, 5.0);
}

float G_Smith(float roughness, float ndotv,float ndotl)
{
	// = G_Schlick / (4 * ndotv * ndotl)

	//for analytical light
	//float a = roughness + 1.0;
	//float k = a * a * 0.125;
	float k = roughness*roughness*0.5;
	float Vis_SchlickV = ndotv/(ndotv * (1 - k) + k);
	float Vis_SchlickL = ndotl/(ndotl * (1 - k) + k);

	return (Vis_SchlickV * Vis_SchlickL);
}
 vec3 ImportanceSampleGGX( vec2 Xi, float Roughness, vec3 N )
{
	float a = Roughness * Roughness;
	float Phi = 2 * PI * Xi.x;
	float CosTheta = sqrt( (1 - Xi.y) / ( 1 + (a*a - 1) * Xi.y ) );
	float SinTheta = sqrt( 1 - CosTheta * CosTheta );
	vec3 H;
	H.x = SinTheta * cos( Phi );
	H.y = SinTheta * sin( Phi );
	H.z = CosTheta;
	vec3 UpVector = abs(N.z) < 0.999 ? vec3(0,0,1) : vec3(1,0,0);

	vec3 TangentX = normalize( cross( UpVector, N ) );
	vec3 TangentY = cross( N, TangentX );
	// Tangent to world space
	return TangentX * H.x + TangentY * H.y + N * H.z;
}
vec3 SpecularIBL( vec3 SpecularColor, float Roughness, vec3 N, vec3 V )
{
	vec3 SpecularLighting = vec3(0);
	const uint NumSamples = 1024;
	float count=0;
	for( uint i = 0; i < NumSamples; i++ )
	{
		vec2 Xi = Hammersley( i, NumSamples );
		vec3 H = ImportanceSampleGGX( Xi, Roughness, N );
		vec3 L = 2 * dot( V, H ) * H - V;
		float NoV = clamp( dot( N, V ),1e-5,1.0 );
		float NoL = clamp( dot( N, L ),0.0,1.0 );
		float NoH = clamp( dot( N, H ),1e-5,1.0 );
		float VoH = clamp( dot( V, H ),1e-5,1.0 );
		//SpecularLighting+=NoV;
		if( NoL > 0 )
		{
			vec3 SampleColor = textureLod(env_map, L, 0 ).rgb;
			float G = G_Smith( Roughness, NoV, NoL );
			float Fc = pow( 1 - VoH, 5 );
			vec3 F = (1 - Fc) * SpecularColor + Fc;
			//vec3 F = fresnel_schlick(SpecularColor,VoH);
			// Incident light = SampleColor * NoL
			// Microfacet specular = D*G*F / (4*NoL*NoV)
			// pdf = D * NoH / (4 * VoH)
			SpecularLighting += SampleColor * F * G * VoH / (NoH * NoV);
		}

	}

	return SpecularLighting / NumSamples;
}
vec3 prefilter_irradiance(vec3 N_world){
	return textureLod(env_map_filtered, N_world, 0.0).rgb;
}

vec3 Diffuse_Lambert( vec3 DiffuseColor )
{
	return DiffuseColor * PI_INV;
}

void main(){

	vec3 N=normalize(Normal);
	vec3 V = normalize(eyepos - Position);

	float metalness=ambient_color.x;
	float roughness=ambient_color.y;


	vec3 base_color=diffuse_color.xyz;

	vec3 color=mix(base_color,vec3(0),metalness);
	vec3 f0=mix(vec3(0.04),base_color,metalness);

	// vec3 f0=vec3(0.04);

	// if(metalness>0.5){
	// 	f0=base_color;
	// 	base_color=vec3(0);
	// }

	vec3 diffuse= Diffuse_Lambert(color)*prefilter_irradiance(N);
	vec3 specular=SpecularIBL(f0,roughness,N,V);


	image_out=specular+diffuse;
	//image_out=base_color;
	//image_out=pow(image_out,vec3(1/2.2));
	//image_out=vec3(roughness);
	//image_out=Normal;
	//image_out=fresnel_schlick(vec3(0.04),dot(V,Normal));
	//image_out=SpecularIBL(vec(1.0),)
}