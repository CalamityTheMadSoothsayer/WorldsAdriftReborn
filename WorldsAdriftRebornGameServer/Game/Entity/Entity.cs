﻿using Improbable.Collections;
using Improbable.Corelibrary.Math;
using Improbable.Corelibrary.Transforms;
using Improbable.Math;

namespace WorldsAdriftRebornGameServer.Game.Entity
{
    public class CoreEntity : Improbable.Worker.Entity
    {
        private const int FRACTIONAL_BITS = 16; // Assuming 16 fractional bits, adjust as needed
        private static long nextEntityId = 1;

        public string? Key { get; set; }

        private Improbable.Collections.List<long>? position;

        public Improbable.Collections.List<long>? Position
        {
            get => position;
            set {
                if (value == null)
                {
                    throw new ArgumentException();
                }
                
                position = new Improbable.Collections.List<long>
                {
                    (value[0] << FRACTIONAL_BITS) / FRACTIONAL_BITS,
                    (value[1] << FRACTIONAL_BITS) / FRACTIONAL_BITS,
                    value[2]
                };
                
                if (Contains<TransformState>()) 
                    Update(new TransformState.Update().SetLocalPosition(new FixedPointVector3(position)));
                else 
                    Add<TransformState>(new TransformState.Data(new FixedPointVector3(position), new Quaternion32(1), null, new Improbable.Math.Vector3d(0f, 0f, 0f), new Improbable.Math.Vector3f(0f, 0f, 0f), new Improbable.Math.Vector3f(0f, 0f, 0f), false, 0f));
            }
        }

        public Option<Vector3d> ToVector3d() => Position == null ? null : new Vector3d(Position[0], Position[1], Position[2]);

        public static long GenerateNextId()
        {
            return nextEntityId++;
        }

        public long Id { get; private set; }

        public virtual void Awake()
        {
            Id = GenerateNextId();
            EntityManager.GlobalEntityRealm[Id] = this;
            Console.WriteLine($"Entity {Id} of type {GetType().Name} added to GlobalEntityRealm");
        }

        public virtual Player? ToPlayer() => null;
    }
}
