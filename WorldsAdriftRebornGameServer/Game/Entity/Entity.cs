using Improbable.Worker;

namespace WorldsAdriftRebornGameServer.Game.Entity
{
    public class CoreEntity : Improbable.Worker.Entity
    {

        private static long nextEntityId = 1;
        private static int fractionalBits = 16; // Assuming 16 fractional bits, adjust as needed

        public virtual string? Key { get; set; }

        private Improbable.Collections.List<long>? _position;

        public virtual Improbable.Collections.List<long>? Position
        {
            get => _position;
            set => _position = ApplyPositionCompensation(value);
        }

        private Improbable.Collections.List<long>? ApplyPositionCompensation( Improbable.Collections.List<long>? rawPosition )
        {
            if (rawPosition == null)
            {
                return null;
            }

            return new Improbable.Collections.List<long>
            {
                (rawPosition[0] << fractionalBits) / fractionalBits,
                (rawPosition[1] << fractionalBits) / fractionalBits,
                rawPosition[2]
            };
        }

        public static long GenerateNextId()
        {
            return nextEntityId++;
        }

        public long Id { get; private set; }
        public readonly Dictionary<uint, IComponentData<IComponentMetaclass>> Components = new Dictionary<uint, IComponentData<IComponentMetaclass>>();

        public virtual void Awake()
        {
            Id = GenerateNextId();
            EntityManager.GlobalEntityRealm[Id] = this;
            Console.WriteLine($"Entity {Id} of type {GetType().Name} added to GlobalEntityRealm");
        }
    }
}
