Shader "Unlit/VoxelShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Volume("Volume", 3D) = "" {}
        _Extents("Extents", Vector) = (0.0, 0.0, 0.0, -1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM

            sampler3D _Volume;

            float3 _Extents;

            struct Ray {
              float3 origin;
              float3 dir;
            };

            struct AABB {
              float3 min;
              float3 max;
            };

            struct AxesAlignedBBox {
              float3 top;
              float3 bottom;
            };

            void rayBoxIntersection(Ray ray, AxesAlignedBBox box, out float t0, out float t1)
            {
              float3 directionInv = 1.0 / ray.dir;
              float3 t_top = directionInv * (box.top - ray.origin);
              float3 t_bottom = directionInv * (box.bottom - ray.origin);
              float3 t_min = min(t_top, t_bottom);
              float2 t = max(t_min.xx, t_min.yz);
              t0 = max(0.0, max(t.x, t.y));
              float3 t_max = max(t_top, t_bottom);
              t = min(t_max.xx, t_max.yz);
              t1 = min(t.x, t.y);
            }

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 world : TEXCOORD1;
                float3 local : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.local = v.vertex.xyz;
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float stepSize = .01; // pass from outside // can be used for LOD
                Ray ray;
                ray.origin = i.local;

                float3 dir = (i.world - _WorldSpaceCameraPos);
                ray.dir = normalize(mul(unity_WorldToObject, dir));

                float3 top = -1.0 * _Extents; //_Top; // would be good if these are passed from outside
                float3 bottom = _Extents; //_Bottom;

                float t0, t1;
                AxesAlignedBBox bbox;
                bbox.top = top;
                bbox.bottom = bottom;
                rayBoxIntersection(ray, bbox, t0, t1);

                float3 rayStart = (ray.origin + ray.dir * t0 - bottom) / (top - bottom);
                float3 rayStop = (ray.origin + ray.dir * t1 - bottom) / (top - bottom);

                float3 rayvec = rayStop - rayStart;
                float rayLen = length(rayvec);
                float3 stepVector = stepSize * rayvec / rayLen;

                float3 position = rayStart;

                float maximum_intensity = 0.0;

                // Run a ray march until we reach end of volume
                // step size decides in how many small intervals we do it
                int iter = 0;
                [unroll]
                while (rayLen > 0.0 && iter < 100) {

                  float intensity = tex3D(_Volume, position).r;

                  if (intensity > maximum_intensity) {
                    maximum_intensity = intensity;
                  }

                  rayLen -= stepSize;
                  position += stepVector;
                  iter++;
                }

                if (maximum_intensity <= .0001) {
                  discard;
                }

                float cl = maximum_intensity * 1.0;
                fixed4 col = fixed4(cl, 0.0, 0.0, 1.0);
                //col = fixed4(0.0, 0.0, 1.0, 1.0);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
