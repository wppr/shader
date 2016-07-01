

in vec2 texcoord;
uniform samplerCube	env_map;
uniform float roughness;
uniform int face;
uniform int UseLatitude;
out vec4 color;

#define PI										3.14159265358979323846

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
 vec3 hemisphereSample_cos( vec2 Xi, float Roughness, vec3 N )
{
	float a = Roughness * Roughness;
	float Phi = 2 * PI * Xi.x;
	float CosTheta = sqrt(1 - Xi.y);
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

vec3 PrefilterEnvMap( float Roughness, vec3 R )
{
	vec3 N = R;
	vec3 V = R;
	vec3 PrefilteredColor = vec3(0);
	float TotalWeight=0;
	const uint NumSamples = 1024;
	for( uint i = 0; i < NumSamples; i++ )
	{
		vec2 Xi = Hammersley( i, NumSamples );
		vec3 H = ImportanceSampleGGX( Xi, Roughness, N );
		vec3 L = 2 * dot( V, H ) * H - V;
		float NoL = clamp( dot( N, L ),0.0,1.0 );
		if( NoL > 0 )
		{
			PrefilteredColor += textureLod(env_map, L, 0 ).rgb * NoL;
			TotalWeight += NoL;
		}
	}
	return PrefilteredColor / TotalWeight;
}
vec3 LatitudeToDir(vec2 l){
	float theta=l.x*2*PI-PI;
	float phi=l.y*PI;
	vec3 R;
	R.y=-cos(phi);
	R.z=sin(phi)*cos(theta);
	R.x=sin(phi)*sin(theta);
	return R;
}
void main(){
	
	vec2 p=texcoord*2-1.0;
	//to save image for opengl use
	//p.y=-p.y;


	vec3 R_px=normalize(vec3(1.0,p.y,-p.x));
	vec3 R_nx=normalize(vec3(-1.0,p.y,p.x));
	vec3 R_py=normalize(vec3(p.x,1.0,-p.y));
	vec3 R_ny=normalize(vec3(p.x,-1.0,p.y));
	vec3 R_pz=normalize(vec3(p.x,p.y,1.0));
	vec3 R_nz=normalize(vec3(-p.x,p.y,-1.0));

	vec3 R=R_px;
	if(face==0) R=R_px;
	if(face==1) R=R_nx;
	if(face==2) R=R_py;	
	if(face==3) R=R_ny;	
	if(face==4) R=R_pz;	
	if(face==5) R=R_nz;	

	if(UseLatitude==1)
		R=LatitudeToDir(texcoord);
	color=vec4(PrefilterEnvMap(roughness,R),1.0);
	//color=textureLod(env_map, R, 0.0).rgb;
	//color=pow(color,vec3(1/2.2));
	//color=vec3(texcoord,0.0);
}