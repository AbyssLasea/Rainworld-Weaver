using RWCustom;
using System.Collections.Generic;
using UnityEngine;

namespace weaver
{
    public class Tongue
    {
        public bool Attached
        {
            get
            {
                return this.mode == weaver.state.Mode.AttachedToTerrain || this.mode == weaver.state.Mode.AttachedToObject;
            }
        }
        public void Update()
        {
            this.lastPos = this.pos;
            this.pos += this.vel;
            if (this.mode == weaver.state.Mode.Retracted)
            {
                this.requestedRopeLength = 0f;
                this.pos = this.worm.mainBodyChunk.pos;
                this.vel = this.worm.mainBodyChunk.vel;
                this.rope.Reset();
            }
            else if (this.mode == weaver.state.Mode.ShootingOut)
            {
                this.requestedRopeLength = Mathf.Max(0f, this.requestedRopeLength - 4f);
                bool flag = false;
                if (!Custom.DistLess(this.baseChunk.pos, this.pos, 60f))
                {
                    Vector2 vector = this.pos + this.vel;
                    SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, this.room, this.pos, ref vector, 1f, 1, this.baseChunk.owner, false);
                    if (collisionResult.chunk != null)
                    {
                        this.AttachToChunk(collisionResult.chunk);
                        flag = true;
                    }
                }
                if (!flag)
                {
                    if (this.worm.playerCheatAttachPos != null && Custom.DistLess(this.pos, this.worm.playerCheatAttachPos.Value, 60f))
                    {
                        Debug.Log("attach to player cheat pos");
                        this.AttachToTerrain(this.worm.playerCheatAttachPos.Value);
                        this.worm.playerCheatAttachPos = null;
                        flag = true;
                    }
                    else
                    {
                        IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(this.room, this.lastPos, this.pos);
                        if (intVector != null)
                        {
                            FloatRect floatRect = Custom.RectCollision(this.pos, this.lastPos, this.room.TileRect(intVector.Value).Grow(1f));
                            this.AttachToTerrain(new Vector2(floatRect.left, floatRect.bottom));
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        this.vel.y = this.vel.y - 0.9f * Mathf.InverseLerp(0.8f, 0f, this.elastic);
                        if (this.returning)
                        {
                            List<IntVector2> list = SharedPhysics.RayTracedTilesArray(this.lastPos, this.pos);
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (this.room.GetTile(list[i]).horizontalBeam)
                                {
                                    this.AttachToTerrain(new Vector2(Mathf.Clamp(Custom.HorizontalCrossPoint(this.lastPos, this.pos, this.room.MiddleOfTile(list[i]).y).x, this.room.MiddleOfTile(list[i]).x - 10f, this.room.MiddleOfTile(list[i]).x + 10f), this.room.MiddleOfTile(list[i]).y));
                                    break;
                                }
                                if (this.room.GetTile(list[i]).verticalBeam)
                                {
                                    this.AttachToTerrain(new Vector2(this.room.MiddleOfTile(list[i]).x, Mathf.Clamp(Custom.VerticalCrossPoint(this.lastPos, this.pos, this.room.MiddleOfTile(list[i]).x).y, this.room.MiddleOfTile(list[i]).y - 10f, this.room.MiddleOfTile(list[i]).y + 10f)));
                                    break;
                                }
                            }
                            if (Custom.DistLess(this.baseChunk.pos, this.pos, 40f))
                            {
                                this.mode = weaver.state.Mode.Retracted;
                            }
                        }
                        else if (Vector2.Dot(Custom.DirVec(this.baseChunk.pos, this.pos), this.vel.normalized) < 0f)
                        {
                            this.returning = true;
                        }
                    }
                }
            }
            else if (this.mode == weaver.state.Mode.Retracting)
            {
                this.mode = weaver.state.Mode.Retracted;
            }
            else if (this.mode == weaver.state.Mode.AttachedToTerrain)
            {
                if (ModManager.MMF && this.worm.room != null)
                {
                    for (int j = 0; j < this.worm.room.zapCoils.Count; j++)
                    {
                        ZapCoil zapCoil = this.worm.room.zapCoils[j];
                        if (zapCoil.turnedOn > 0.5f && zapCoil.GetFloatRect.Vector2Inside(this.terrainStuckPos))
                        {
                            zapCoil.TriggerZap(this.terrainStuckPos, 4f);
                            this.worm.mainBodyChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(this.terrainStuckPos, this.worm.mainBodyChunk.pos)).normalized * 8f;
                            this.Release();
                            this.worm.room.AddObject(new ZapCoil.ZapFlash(this.worm.mainBodyChunk.pos, 6f));
                            this.worm.Die();
                        }
                    }
                }
                this.pos = this.terrainStuckPos;
                this.vel *= 0f;
                if (this.worm.noSpearStickZones.Count > 0 && !Custom.DistLess(this.pos, this.worm.mainBodyChunk.pos, 20f) && UnityEngine.Random.value < 0.1f)
                {
                    for (int k = 0; k < this.worm.noSpearStickZones.Count; k++)
                    {
                        if (Custom.DistLess(this.pos, this.worm.noSpearStickZones[k].pos, (this.worm.noSpearStickZones[k].data as PlacedObject.ResizableObjectData).Rad))
                        {
                            this.Release();
                            break;
                        }
                    }
                }
            }
            else if (this.mode == weaver.state.Mode.AttachedToObject)
            {
                if (this.attachedChunk != null)
                {
                    this.pos = this.attachedChunk.pos;
                    this.vel = this.attachedChunk.vel;
                    if (this.attachedChunk.owner.room != this.room)
                    {
                        this.attachedChunk = null;
                        this.mode = weaver.state.Mode.Retracting;
                    }
                }
                else
                {
                    this.mode = weaver.state.Mode.Retracting;
                }
            }
            this.rope.Update(this.baseChunk.pos, this.pos);
            if (this.mode != weaver.state.Mode.Retracted)
            {
                this.Elasticity();
            }
            if (this.Attached)
            {
                this.elastic = Mathf.Max(0f, this.elastic - 0.05f);
                if (this.elastic <= 0f)
                {
                    this.ropeExtendSpeed = Mathf.Min(this.ropeExtendSpeed + 0.025f, 1f);
                }
                if (this.requestedRopeLength < this.idealRopeLength)
                {
                    this.requestedRopeLength = Mathf.Min(this.requestedRopeLength + this.ropeExtendSpeed * 2f, this.idealRopeLength);
                    return;
                }
                if (this.requestedRopeLength > this.idealRopeLength)
                {
                    this.requestedRopeLength = Mathf.Max(this.requestedRopeLength - (1f - this.elastic) * 2f, this.idealRopeLength);
                    return;
                }
            }
            else
            {
                this.ropeExtendSpeed = 0f;
            }
        }
        public bool Free
        {
            get
            {
                return this.mode == weaver.state.Mode.ShootingOut || this.mode == weaver.state.Mode.Retracting;
            }
        }

        private void AttachToTerrain(Vector2 attPos)
        {
            this.terrainStuckPos = attPos;
            this.mode = weaver.state.Mode.AttachedToTerrain;
            this.pos = this.terrainStuckPos;
            this.Attatch();
            this.room.PlaySound(SoundID.Tube_Worm_Tongue_Hit_Terrain, this.pos);
        }

        private void Attatch()
        {
            this.vel *= 0f;
            this.elastic = 1f;
            this.requestedRopeLength = Vector2.Distance(this.baseChunk.pos, this.pos);
        }
        public void Release()
        {
            if (this.mode == weaver.state.Mode.AttachedToObject && this.attachedChunk != null)
            {
                this.room.PlaySound(SoundID.Tube_Worm_Detatch_Tongue_Creature, this.pos);
            }
            else if (this.mode == weaver.state.Mode.AttachedToTerrain)
            {
                this.room.PlaySound(SoundID.Tube_Worm_Detach_Tongue_Terrain, this.pos);
            }
            this.mode = weaver.state.Mode.Retracting;
            this.attachedChunk = null;
        }

        private void AttachToChunk(BodyChunk chunk)
        {
            this.attachedChunk = chunk;
            this.pos = chunk.pos;
            this.mode = weaver.state.Mode.AttachedToObject;
            this.Attatch();
            this.room.PlaySound(SoundID.Tube_Worm_Tongue_Hit_Creature, this.pos);
        }
        private void Elasticity()
        {
            float num = 0f;
            if (this.mode == weaver.state.Mode.AttachedToTerrain)
            {
                num = 1f;
            }
            else if (this.mode == weaver.state.Mode.AttachedToObject)
            {
                num = this.attachedChunk.mass / (this.attachedChunk.mass + this.baseChunk.mass);
            }
            Vector2 a = Custom.DirVec(this.baseChunk.pos, this.rope.AConnect);
            float totalLength = this.rope.totalLength;
            float a2 = 0.7f;
            if (this.worm.tongues[0].Attached && this.worm.tongues[1].Attached)
            {
                a2 = Custom.LerpMap(Mathf.Abs(0.5f - this.worm.onRopePos), 0.5f, 0.4f, 1.1f, 0.7f);
            }
            float num2 = this.worm.RequestRope(this.tongueNum) * Mathf.Lerp(a2, 1f, this.elastic);
            float d = Mathf.Lerp(0.85f, 0.25f, this.elastic);
            if (totalLength > num2)
            {
                this.baseChunk.vel += a * (totalLength - num2) * d * num;
                this.baseChunk.pos += a * (totalLength - num2) * d * num * Mathf.Lerp(1f, (this.mode == weaver.state.Mode.AttachedToTerrain) ? 1f : 0.5f, this.elastic);
                a = Custom.DirVec(this.pos, this.rope.BConnect);
                if (this.Free)
                {
                    this.vel += a * (totalLength - num2) * d * (1f - num);
                    this.pos += a * (totalLength - num2) * d * (1f - num) * Mathf.Lerp(1f, 0.5f, this.elastic);
                    return;
                }
                if (this.mode == weaver.state.Mode.AttachedToObject)
                {
                    this.attachedChunk.vel += a * (totalLength - num2) * d * (1f - num);
                    this.attachedChunk.pos += a * (totalLength - num2) * d * (1f - num) * Mathf.Lerp(1f, 0.5f, this.elastic);
                }
            }
        }
        public Vector2 pos;
        public Vector2 lastPos;
        public Vector2 vel;
        public BodyChunk baseChunk;
        public TubeWorm worm;
        public Room room;
        public int tongueNum;
        public Vector2 terrainStuckPos;
        public BodyChunk attachedChunk;
        public float myMass = 0.1f;
        public bool returning;
        public float requestedRopeLength;
        public float idealRopeLength;
        public float elastic;
        public float ropeExtendSpeed;
        public Rope rope;
        public weaver.state.Mode mode;
    }
}
