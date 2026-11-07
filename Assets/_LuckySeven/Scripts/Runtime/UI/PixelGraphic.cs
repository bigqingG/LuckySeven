using UnityEngine;
using UnityEngine.UI;

namespace LuckySeven.UI
{
    // Reusable pixel UI geometry; no texture/material allocations in the render loop.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PixelGraphic : MaskableGraphic
    {
        public enum Shape { Frame, Stone, Coin, Shield, Sword, Echo, Slash, Fragment }
        public Shape shape;
        public Color fill = new Color32(34,43,69,255);
        public static readonly Color Navy=new Color32(25,33,55,255), Slate=new Color32(46,57,84,255),
            Paper=new Color32(246,229,188,255), Gold=new Color32(222,174,96,255),
            Ink=new Color32(27,31,45,255), Red=new Color32(133,48,51,255), Teal=new Color32(49,99,116,255);
        public void SetFill(Color value){if(fill==value)return;fill=value;SetVerticesDirty();}
        public static void Quad(VertexHelper v,float x,float y,float w,float h,Color c)
        {
            int n=v.currentVertCount;
            v.AddVert(new Vector3(x,y),c,Vector2.zero);v.AddVert(new Vector3(x+w,y),c,Vector2.zero);
            v.AddVert(new Vector3(x+w,y-h),c,Vector2.zero);v.AddVert(new Vector3(x,y-h),c,Vector2.zero);
            v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);
        }
        public static void Step(VertexHelper v,float x,float y,float w,float h,float cut,Color c)
        {
            Quad(v,x+cut,y,w-2*cut,h,c);Quad(v,x,y-cut,cut,h-2*cut,c);Quad(v,x+w-cut,y-cut,cut,h-2*cut,c);
            Quad(v,x+cut*.5f,y-cut*.5f,cut*.5f,h-cut,c);Quad(v,x+w-cut,y-cut*.5f,cut*.5f,h-cut,c);
        }
        public static void Mask(VertexHelper v,string[] rows,float x,float y,float size,Color ink)
        {for(int r=0;r<rows.Length;r++)for(int c=0;c<rows[r].Length;c++)if(rows[r][c]!='.')Quad(v,x+c*size,y-r*size,size,size,ink);}
        public static readonly string[][] Suits={
            new[]{"...##...","..####..",".######.","########","########",".##..##.","...##...","..####.."},
            new[]{".##..##.","########","########","########",".######.","..####..","...##...","........"},
            new[]{"...##...","..####..",".######.","########",".######.","..####..","...##...","........"},
            new[]{"..####..","..####..","...##...",".######.","########",".##..##.","...##...","..####.."}};
        public static readonly string[] Shield={".######.","########","##.##.##","##.##.##","########",".######.","..####..","...##..."},
            Sword={"......##",".....###","....###.",".#.###..",".####...","..##....",".####...","##..#..."},
            Echo={"..#####.",".##...##","##....##","##..####","####....","##....##",".##..##.","..####.."};
        static readonly string[][] Digits={
            new[]{".###.","##.##","##.##","##.##","##.##","##.##",".###."},
            new[]{"..#..",".##..","..#..","..#..","..#..","..#..",".###."},
            new[]{".###.","##.##","...##","..##.",".##..","##...","#####"},
            new[]{"####.","...##","...##",".###.","...##","...##","####."},
            new[]{"...##","..###",".####","##.##","#####","...##","...##"},
            new[]{"#####","##...","####.","...##","...##","##.##",".###."},
            new[]{".###.","##...","##...","####.","##.##","##.##",".###."},
            new[]{"#####","...##","..##.","..##.",".##..",".##..",".##.."},
            new[]{".###.","##.##","##.##",".###.","##.##","##.##",".###."},
            new[]{".###.","##.##","##.##",".####","...##","...##",".###."}};
        public static void Digit(VertexHelper v,int number,float x,float y,float size,Color ink)=>Mask(v,Digits[Mathf.Clamp(number,0,9)],x,y,size,ink);
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();var r=rectTransform.rect;float x=r.xMin,y=r.yMax,w=r.width,h=r.height;
            if(shape==Shape.Stone){
                Quad(v,x,y,w,h,Navy*color);
                for(int row=0;row<Mathf.CeilToInt(h/38);row++)for(int col=0;col<Mathf.CeilToInt(w/84)+1;col++){
                    float bx=x+col*84+(row%2)*-38;float by=y-row*38;
                    float bw=Mathf.Min(79,x+w-bx);if(bx<x){bw-=x-bx;bx=x;}if(bw<10)continue;
                    var c=Color.Lerp(Navy,Slate,.2f+((row*7+col*3)%5)*.04f)*color;
                    Step(v,bx+2,by-2,bw-3,32,4,c);Quad(v,bx+8,by-4,Mathf.Max(0,bw-19),2,new Color32(56,67,91,255)*color);
                }return;
            }
            if(shape==Shape.Frame||shape==Shape.Coin){
                float cut=shape==Shape.Coin?Mathf.Min(w,h)*.16f:8;
                Step(v,x,y,w,h,cut,Ink*color);Step(v,x+2,y-2,w-4,h-4,cut,Gold*color);
                Step(v,x+4,y-4,w-8,h-8,cut,Paper*color);Step(v,x+6,y-6,w-12,h-12,cut,Gold*color);
                Step(v,x+8,y-8,w-16,h-16,Mathf.Max(2,cut-3),fill*color);
                if(shape==Shape.Frame&&w>170&&h>80){Quad(v,x+15,y-12,22,2,Gold*color);Quad(v,x+w-37,y-h+14,22,2,Gold*color);}
                return;
            }
            if(shape==Shape.Fragment){Step(v,x,y,w,h,3,Gold*color);return;}
            if(shape==Shape.Slash){for(int i=0;i<18;i++){float size=(1-Mathf.Abs(i-8.5f)/10)*h*.22f;Quad(v,x+w*i/18,y-h+h*i/18,w/12,size,Paper*color);}return;}
            var mask=shape==Shape.Shield?Shield:shape==Shape.Sword?Sword:Echo;
            float scale=Mathf.Floor(Mathf.Min(w,h)/8);Mask(v,mask,x+(w-8*scale)/2,y-(h-8*scale)/2,scale,fill*color);
        }
    }
}

