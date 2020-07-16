﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[InternalBufferCapacity(15)]
public struct Seat : IBufferElementData
{
    public Entity occupiedBy;
}