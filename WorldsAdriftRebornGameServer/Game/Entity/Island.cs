using Bossa.Travellers.World;
using Improbable;
using Improbable.Collections;
using Improbable.Math;

namespace WorldsAdriftRebornGameServer.Game.Entity
{
    // Custom class to represent the association between an island and its respawners
    public class IslandRespawnerAssociation
    {
        public long IslandId { get; set; }
        public EntityId FirstRespawner { get; set; }
    }

    public class Island : CoreEntity
    {
        public static readonly System.Collections.Generic.List<Island> IslandList = new System.Collections.Generic.List<Island>();
        public static readonly Improbable.Collections.List<IslandRespawnerAssociation> IslandSpawners = new Improbable.Collections.List<IslandRespawnerAssociation>();

        public override void Awake()
        {
            base.Awake();
            IslandList.Add(this);
            Console.WriteLine($"Island {Id} added to IslandList");
            AddAncientRespawnerToIslandSpawners();
        }

        private void AddAncientRespawnerToIslandSpawners()
        {
            // Assuming AncientRespawner.ARlist is a List<EntityId> containing available respawners
            if (AncientRespawner.ARlist.Count >= 1)
            {
                EntityId firstRespawner = AncientRespawner.ARlist[0];

                var association = new IslandRespawnerAssociation
                {
                    IslandId = Id,
                    FirstRespawner = firstRespawner
                };

                IslandSpawners.Add(association);

                Console.WriteLine($"Added AncientRespawners to IslandSpawners for Island {Id}: {firstRespawner.Id}");

                // Remove the used respawners from AncientRespawner.ARlist
                AncientRespawner.ARlist.RemoveAt(0);

                Console.WriteLine($"Removed used AncientRespawners from ARlist.");
            }
            else
            {
                Console.WriteLine("Not enough AncientRespawners available to add to IslandSpawners.");
            }
        }
    }
}
