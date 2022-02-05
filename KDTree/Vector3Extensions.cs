namespace Supercluster.KDTree;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

public static class Vector3Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref float DimRef(ref this Vector3 vec, int dim)
    {
        switch (dim)
        {
            case 0:
                return ref vec.X;
            case 1:
                return ref vec.Y;
            case 2:
                return ref vec.Z;
            default:
                throw new ArgumentOutOfRangeException(nameof(dim));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dim(this Vector3 vec, int dim)
    {
        return dim switch
        {
            0 => vec.X,
            1 => vec.Y,
            2 => vec.Z,
            _ => throw new ArgumentOutOfRangeException(nameof(dim))
        };
    }
}