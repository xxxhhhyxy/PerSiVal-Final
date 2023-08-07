// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/PointCloud"
{
    Properties{
        _PointSize ("Point Size",Range(0,10)) = 2.0
    }

    SubShader{
    Pass{    
        LOD 200

        CGPROGRAM
        // vertex shader
        #pragma vertex vert
        #pragma fragment frag

        struct VertexInput{
            float4 v : POSITION;
            float4 color : COLOR;
        };

        struct VertexOutput{
            float4 pos : SV_POSITION;
            float4 col : COLOR;
            float pointSize : PSIZE;
        };

        float _PointSize;

        VertexOutput vert(VertexInput v){
            VertexOutput o;
            o.pos = UnityObjectToClipPos(v.v);
            o.col = v.color;
            o.pointSize = _PointSize;
            return o;
        }

        float4 frag(VertexOutput o) : COLOR{
            return o.col;
        }

        ENDCG
    }
    }
}