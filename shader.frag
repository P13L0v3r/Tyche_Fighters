#version 330 core

in float shade;

out vec4 FragColor;

void main()
{
    FragColor = vec4(shade, shade, shade, 1.0f);
}