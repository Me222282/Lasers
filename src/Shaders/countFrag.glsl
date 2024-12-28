#version 430 core

layout(location = 0) out vec4 colour;

uniform float value;

void main()
{
	colour = vec4(value);
}