using UnityEngine;
using UnityEngine.UIElements;

public static class CustomNoise
{
    public static float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        // scale 값에 따라 작을 수록 부드러운 언덕 높을수록 거친 지형(산악 지형)이 형성됨
        // offset 값은 Perlin Noise의 시작 위치를 이동하는 역할 즉, 같은 scale값을 사용하더라도 완전히 다른 지형이 생성됨
        // 정리해서 offset을 0으로 주게 되면 다른 서버에 다른 월드에 같은 좌표에서는 똑같은 높이 값이 나오게 됨
        // 서버마다 다른 월드 생성 가능한 이유가 offset 때문이다.
        // normalized를 해주기 위해 ChunkWidth, ChunkDepth값을 나눈다.
        return Mathf.PerlinNoise(
            (position.x + 0.1f) / VoxelData.ChunkWidth * scale + offset,
            (position.y + 0.1f) / VoxelData.ChunkDepth * scale + offset);
    }

    public static bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold)
    {
        float x = (position.x + offset +0.1f) * scale;
        float y = (position.y + offset +0.1f) * scale;
        float z = (position.z + offset +0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);
        if ((AB + BC + AC + BA + CB + CA + CA) / 6 > threshold)
            return true;
        else
            return false;
    }
}
