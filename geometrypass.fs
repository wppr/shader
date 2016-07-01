
in vec3 v_normal;
in vec2 v_texcoord;
in vec3 pos_view;

uniform vec4 diffuse_color;
uniform sampler2D diffuse_texture;					

uniform vec4 specular_color;
uniform sampler2D specular_texture;

uniform vec4 ambient_color;
uniform sampler2D ambient_texture;

layout (location = 0) out vec4 DiffuseOut; 
layout (location = 1) out vec4 SpeculerOut; 
layout (location = 2) out uvec2 NormalOut; 
layout (location = 3) out vec4 NormalOut2; 



void main() 
{ 

    //diffuse out
    DiffuseOut = texture(diffuse_texture, v_texcoord)*diffuse_color;

    //specular out
   SpeculerOut = texture(specular_texture, v_texcoord)*specular_color;

    //ambient out
    SpeculerOut.w=dot(ambient_color.rgb, vec3(0.2126, 0.7152, 0.0722));

    //normal out
    // vec3 N=normalize(v_normal)*0.5+0.5; 
    // vec2 n1=EncodeFloat2Byte(N.x);
    // vec2 n2=EncodeFloat2Byte(N.y);
    // vec2 n3=EncodeFloat2Byte(N.z);
    
    //NormalOut=vec4(n1.x,n1.y,n2.x,n2.y);

 //   NormalOut2=vec4(n3.x,n3.y,1.0,1.0);

    NormalOut2=vec4(normalize(v_normal),1.0);

    NormalOut=uvec2(packHalf2x16(v_normal.xy), packHalf2x16(vec2(v_normal.z, 0.0)));

    // NormalOut2=vec4(decode(n1),decode(n2),decode(n3),1.0);
    // //float
    // NormalOut=vec4(normalize(v_normal),1.0);
    // NormalOut2=vec4(pos_view,1.0);
}
	

     