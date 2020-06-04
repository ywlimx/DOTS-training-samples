﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[GenerateAuthoringComponent]
[WriteGroup(typeof(LocalToWorld))]
public struct Rotation2D : IComponentData
{
    public float Value;
}