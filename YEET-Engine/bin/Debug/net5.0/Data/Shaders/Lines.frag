﻿#version 330 core

out vec4 FragColor;
in vec3 rgb;

void main()
{
        FragColor = vec4(rgb,1.0);
}