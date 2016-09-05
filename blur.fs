in vec2 texcoord;
out vec4 fragout;

//declare uniforms
uniform sampler2D u_texture;

uniform float radius;
uniform vec2 dir;

void main() {
    //this will be our RGBA sum
    vec4 sum = vec4(0.0);

    //our original texcoord for this fragment
    vec2 tc = texcoord;

    //the amount to blur, i.e. how far off center to sample from 
    //1.0 -> blur by one pixel
    //2.0 -> blur by two pixels, etc.

    vec2 resolution=textureSize(u_texture,0);

    vec2 blurSize = vec2(radius)/resolution; 

    //the direction of our blur
    //(1.0, 0.0) -> x-axis blur
    //(0.0, 1.0) -> y-axis blur
    float hstep = dir.x;
    float vstep = dir.y;

    //apply blurring, using a 9-tap filter with predefined gaussian weights

    sum += texture2D(u_texture, vec2(tc.x - 4.0*blurSize.x*hstep, tc.y - 4.0*blurSize.y*vstep)) * 0.0162162162;
    sum += texture2D(u_texture, vec2(tc.x - 3.0*blurSize.x*hstep, tc.y - 3.0*blurSize.y*vstep)) * 0.0540540541;
    sum += texture2D(u_texture, vec2(tc.x - 2.0*blurSize.x*hstep, tc.y - 2.0*blurSize.y*vstep)) * 0.1216216216;
    sum += texture2D(u_texture, vec2(tc.x - 1.0*blurSize.x*hstep, tc.y - 1.0*blurSize.y*vstep)) * 0.1945945946;

    sum += texture2D(u_texture, vec2(tc.x, tc.y)) * 0.2270270270;

    sum += texture2D(u_texture, vec2(tc.x + 1.0*blurSize.x*hstep, tc.y + 1.0*blurSize.y*vstep)) * 0.1945945946;
    sum += texture2D(u_texture, vec2(tc.x + 2.0*blurSize.x*hstep, tc.y + 2.0*blurSize.y*vstep)) * 0.1216216216;
    sum += texture2D(u_texture, vec2(tc.x + 3.0*blurSize.x*hstep, tc.y + 3.0*blurSize.y*vstep)) * 0.0540540541;
    sum += texture2D(u_texture, vec2(tc.x + 4.0*blurSize.x*hstep, tc.y + 4.0*blurSize.y*vstep)) * 0.0162162162;

    fragout=sum;
}