#version 430 core

layout(location = 0) out vec4 colour;

in vec2 tex_Coords;

uniform sampler2D uTextureSlot;

//uniform int screenWidth;
//uniform int screenHeight;

uniform float[9] kernel;
uniform float kernelOffset = 700.0;

void main()
{
    vec2 coords = tex_Coords;

    float offset = 1.0 / kernelOffset;

    vec2 offsets[9] = vec2[](
        vec2(-offset, offset),  // top-left
        vec2(0.0, offset),     // top-center
        vec2(offset, offset),   // top-right
        vec2(-offset, 0.0),    // center-left
        vec2(0.0, 0.0),       // center-center
        vec2(offset, 0.0),     // center-right
        vec2(-offset, -offset), // bottom-left
        vec2(0.0, -offset),    // bottom-center
        vec2(offset, -offset)   // bottom-right    
    );

    vec3 col = vec3(0.0);
    float alpha = 0.0;
    for (int i = 0; i < 9; i++)
    {
        vec4 s = texture(uTextureSlot, coords.st + offsets[i]);
        if (i == 4)
        {
            alpha = s.a;
        }
        col += vec3(s) * kernel[i];
    }

    colour = texture(uTextureSlot, coords.st);
}