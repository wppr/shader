

in vec2 texcoord;
uniform sampler2D test_tex;
uniform sampler2D test_tex2;
uniform samplerCube test_tex_cube;
out vec4 color;

void main() {
  vec4 q = texture(test_tex, texcoord);
  // vec4 p = texture(test_tex2, texcoord);
  // vec4 f=vec4(pow(q.x,0.45),pow(q.y,0.45),pow(q.z,0.45),1.0);
  // vec4 g=texture(test_tex_cube, vec3(texcoord.xy,1.0));
  // float gamma=1.0/1.9;
  // color=vec4(pow(q.r,gamma),pow(q.g,gamma),pow(q.b,gamma),1.0);

  color=vec4(q.xyz,1.0);

  //
 // color=vec4(1-1*(1-q.x));
}