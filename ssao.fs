
in vec2 texcoord;

uniform int num_samples;
uniform vec3 samples[128];

out vec4 ao_color;
uniform sampler2D zbuffer;
uniform sampler2D normal;
//uniform sampler2D normal2;
//uniform highp usampler2D  normal;
uniform sampler2D noise;


uniform mat4 view;
uniform mat4 proj;
uniform mat4 proj_inv_mat;
uniform mat4 proj_view_inv_mat;
float acc_image;
float radius=0.04;

vec4 getPosition(vec2 uv){
  float depth = texture(zbuffer, texcoord).x;
  vec3 ndc = 2.0 * vec3(texcoord, depth) - 1.0;
  vec4 P = proj_inv_mat * vec4(ndc, 1.0); 
  P.xyz /= P.w;
  return P;
}

vec3 getNormal2(vec2 uv){
  vec4 Normalraw = texture(normal, texcoord);
  return normalize(Normalraw.xyz);
}
// vec3 getNormal3(vec2 uv){
//   uvec2 image_packed = texelFetch(normal, ivec2(gl_FragCoord.xy), 0).xy;
//   vec4 image_unpacked = vec4(unpackHalf2x16(image_packed.x), unpackHalf2x16(image_packed.y));
//   return image_unpacked.xyz;
// }
void main() {
  if(num_samples==0)
  {
    ao_color=vec4(1.0);
    return;
  }
  float depth = texture(zbuffer, texcoord).x;
    if (1.0==depth) {
    return;
  }

  vec2 image_scale = 0.33333333* vec2(textureSize(zbuffer, 0));

  vec3 N=getNormal2(texcoord);
  vec3 D =texture(noise, texcoord * image_scale).xyz;

  vec4 P=getPosition(texcoord);

  

 // N.z = sqrt(1.0 - dot(N.xy, N.xy));

  vec3 T = normalize(D - dot(N, D) * N);
  vec3 B = cross(N, T);

  float occ = 0.0;
  for (int i = 0; i < num_samples; ++i) {
    float ratio=1.0/float(num_samples);
    vec3 point_sample = P.xyz+ mat3(T, B, N) * samples[i].xyz* radius;
    vec4 point_sample_clip = proj*vec4(point_sample, 1.0);
    vec3 point_sample_ndc = point_sample_clip.xyz / point_sample_clip.w;
    vec3 coord = 0.5 * point_sample_ndc + 0.5;
    float surface = texture(zbuffer, coord.xy).x;
    vec4 point_sample_checking = proj_inv_mat * (2.0 * vec4(coord.xy, surface, 1.0) - 1.0);
     occ+= step(surface, coord.z) * step(point_sample_checking.z / point_sample_checking.w - P.z, radius / N.z);
  }
  acc_image = 1.0 - (occ / float(num_samples));


  ao_color=vec4(acc_image,acc_image,acc_image,1.0);

}