using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace IslandAdventure.Content.Dusts
{
    public class BunaLeafDust : ModDust
    {
        public override string Texture=>"IslandAdventure/Assets/Dusts/BunaLeaf";
        public override void OnSpawn(Dust dust)
        {
            dust.frame=new Rectangle(0,Main.rand.Next(3)*14,10,14);
            dust.noGravity=dust.noLight=true;dust.alpha=0;
            dust.fadeIn=Main.rand.NextFloat(0,MathHelper.TwoPi);
            dust.scale=Main.rand.NextFloat(.65f,1f);
            dust.velocity.Y=Main.rand.NextFloat(.35f,.75f);
        }
        public override bool Update(Dust dust)
        {
            dust.alpha++;
            float flutter=(float)System.Math.Sin(dust.alpha*.08f+dust.fadeIn);
            dust.velocity.X=MathHelper.Lerp(dust.velocity.X,Main.windSpeedCurrent*1.8f+flutter*.45f,.05f);
            dust.velocity.Y=System.Math.Min(1.3f,dust.velocity.Y+.008f);
            dust.position+=dust.velocity;dust.rotation+=flutter*.035f;
            if(Collision.SolidCollision(dust.position,2,2)){dust.velocity*=.7f;dust.alpha+=12;}
            if(dust.alpha>=240)dust.active=false;
            return false;
        }
        public override Color? GetAlpha(Dust dust,Color lightColor)=>lightColor*(1f-System.Math.Min(1,dust.alpha/240f));
    }
    public class BunaBarkDust : ModDust
    {
        public override string Texture=>"IslandAdventure/Assets/Dusts/BunaBark";
        public override void OnSpawn(Dust dust)
        {dust.frame=new Rectangle(0,0,8,8);dust.noLight=true;dust.noGravity=false;dust.scale=.8f;}
        public override bool Update(Dust dust)
        {
            dust.velocity.Y+=.12f;dust.velocity*=.98f;dust.position+=dust.velocity;
            dust.rotation+=dust.velocity.X*.08f;dust.scale*=.96f;
            if(dust.scale<.2f)dust.active=false;
            return false;
        }
        public override Color? GetAlpha(Dust dust,Color lightColor)=>lightColor;
    }
}
