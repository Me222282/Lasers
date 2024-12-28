#version 430 core

layout(location = 0) out vec4 colour;

in vec2 tex_Coords;

uniform sampler2D uTextureSlot;
uniform float div;
uniform vec3 uColour;

void main()
{
    float alpha = texture(uTextureSlot, tex_Coords).r / div;
    
	colour = vec4(uColour, alpha);
}