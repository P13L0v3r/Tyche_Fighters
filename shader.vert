#version 330 core
layout (location = 0) in vec3 aPosition;

out float shade;

void main()
{
    shade = aPosition.z;
    gl_Position = vec4(aPosition.x, aPosition.y, 0.0, 1.0);
}