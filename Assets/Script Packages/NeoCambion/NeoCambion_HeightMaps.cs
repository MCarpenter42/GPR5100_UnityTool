namespace NeoCambion
{
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    namespace Heightmaps
    {
        public enum HeightMapType { Perlin, Random };

        public class HeightMap
        {
            public HeightMapType mapType = HeightMapType.Perlin;

            private float[,] _map;
            public float[,] Map
            {
                get
                {
                    if (_map == null)
                    {
                        ReGenerate();
                    }
                    return _map;
                }
                set
                {
                    if (!locked)
                    {
                        _map = value;
                    }
                }
            }

            public int width = 10;
            public int depth = 10;
            public float scale = 1.0f;

            public Vector2 offset = Vector2.zero;

            public bool locked = false;

            /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

            public HeightMap(int width, int depth, float scale, Vector2 offset)
            {
                this.width = width;
                this.depth = depth;
                this.scale = scale;
                this.offset = offset;
                ReGenerate();
            }
            
            public HeightMap(int width, int depth, float scale, Vector2 offset, HeightMapType mapType)
            {
                this.width = width;
                this.depth = depth;
                this.scale = scale;
                this.offset = offset;
                this.mapType = mapType;
                ReGenerate();
            }
            
            public HeightMap(float[,] templateMap, bool locked)
            {
                _map = templateMap;
                this.locked = locked;
            }
            
            /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

            public void ReGenerate()
            {
                if (!locked)
                {
                    _map = new float[width, depth];
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < depth; y++)
                        {
                            switch (mapType)
                            {
                                default:
                                case HeightMapType.Perlin:
                                    _map[x, y] = NoisePoint_Perlin(x, y);
                                    break;
                                    
                                case HeightMapType.Random:
                                    _map[x, y] = NoisePoint_Random(x, y);
                                    break;
                            }
                        }
                    }
                }
            }

            public void ReGenerate(int width, int depth, float scale, Vector2 offset)
            {
                if (!locked)
                {
                    this.width = width;
                    this.depth = depth;
                    this.scale = scale;
                    this.offset = offset;

                    ReGenerate();
                }
            }

            float NoisePoint_Perlin(float x, float y)
            {
                float posX = x / width * scale + offset.x;
                float posY = y / depth * scale + offset.y;

                return Mathf.PerlinNoise(posX, posY);
            }

            float NoisePoint_Random(float x, float y)
            {
                return UnityEngine.Random.Range(0.0f, 1.0f);
            }
        }
    }
}