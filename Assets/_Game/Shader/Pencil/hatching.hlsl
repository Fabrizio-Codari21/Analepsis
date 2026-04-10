float3 Hatching_float(float _intensity, float3 hatch0, float3 hatch1, out float3 hatching)
{
    float3 overbright = max(0, _intensity - 1.0);

    float3 weightsA = saturate((_intensity * 6.0) - float3(0, 1, 2));
    float3 weightsB = saturate((_intensity * 6.0) - float3(3, 4, 5));

    weightsA.xy -= weightsA.yz;
    weightsA.z -= weightsB.x;
    weightsB.xy -= weightsB.yz;

    hatch0 = hatch0 * weightsA;
    hatch1 = hatch1 * weightsB;

    hatching = overbright + hatch0.r +
        hatch0.g + hatch0.b +
        hatch1.r + hatch1.g +
        hatch1.b;

    return hatching;
}