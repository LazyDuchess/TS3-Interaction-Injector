using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;

namespace LazyDuchess.InteractionInjection
{
    public static class InteractionInjector
    {
        static EventListener gSimInstantiatedEventListener;
        static readonly Dictionary<Type, List<InteractionDefinition>> gInteractionsToInjectPerGameObject = new Dictionary<Type, List<InteractionDefinition>>();

        /// <summary>
        /// Add an interaction to a GameObject class type.
        /// </summary>
        /// <typeparam name="T">GameObject type to add interaction to.</typeparam>
        /// <param name="interaction">Interaction definition to add to object.</param>
        public static void RegisterInteraction<T>(InteractionDefinition interaction) where T : GameObject
        {
            // Get the interaction list for this GameObject type from the Dictionary, or make it if it doesn't exist.
            var interactionList = MakeInteractionInjectListForType(typeof(T));

            if (interactionList.Contains(interaction))
                return;

            interactionList.Add(interaction);

            if (GameUtils.GetCurrentWorld() == WorldName.Undefined)
                return;

            // Add the new interaction to all existing objects of type, if they don't have it already.
            var objectQuery = Queries.GetObjects(typeof(T));
            foreach (GameObject gameObject in objectQuery)
            {
                foreach (var pair in gameObject.Interactions)
                {
                    if (pair.InteractionDefinition.GetType() == interaction.GetType())
                    {
                        continue;
                    }
                }
                gameObject.AddInteraction(interaction);
            }
        }

        /// <summary>
        /// Remove a previously added interaction to a GameObject class type.
        /// </summary>
        /// <typeparam name="T">GameObject type to remove interaction from.</typeparam>
        /// <param name="interaction">Interaction definition to remove.</param>
        public static void UnregisterInteraction<T>(InteractionDefinition interaction) where T : GameObject
        {
            // Get the interaction list for this GameObject type from the Dictionary. Returns null if there isn't one.
            var interactionList = GetInteractionInjectListForType(typeof(T));

            if (interactionList == null)
                return;

            if (!interactionList.Contains(interaction))
                return;

            interactionList.Remove(interaction);

            if (GameUtils.GetCurrentWorld() == WorldName.Undefined)
                return;

            // Remove the interaction from all existing objects of type.
            var objectQuery = Queries.GetObjects(typeof(T));
            foreach (GameObject gameObject in objectQuery)
            {
                // Make a copy so that we can work on the original without raising exceptions.
                var interactionCache = new List<InteractionObjectPair>(gameObject.Interactions);
                foreach (var interactionPair in interactionCache)
                {
                    if (interactionPair.InteractionDefinition.GetType() == interaction.GetType())
                        gameObject.Interactions.Remove(interactionPair);
                }
            }
        }

        /// <summary>
        /// Call this from entrypoint. Initialize all events.
        /// </summary>
        public static void Initialize()
        {
            World.sOnWorldLoadFinishedEventHandler += OnWorldLoad;
            World.sOnWorldQuitEventHandler += OnWorldQuit;
            World.sOnObjectPlacedInLotEventHandler += OnObjectPlaced;
        }

        static void OnWorldLoad(object sender, EventArgs e)
        {
            // Listen to Sim instantiation to inject interactions for Sim class.
            gSimInstantiatedEventListener = EventTracker.AddListener(EventTypeId.kSimInstantiated, OnSimInstantiated);

            // Add interactions to all objects already placed in the world.
            foreach (var typeInteraction in gInteractionsToInjectPerGameObject)
            {
                var query = Queries.GetObjects(typeInteraction.Key);
                foreach (GameObject gameObject in query)
                {
                    AddInteractions(gameObject, typeInteraction.Value);
                }
            }
        }

        static void OnObjectPlaced(object sender, EventArgs e)
        {
            var args = e as World.OnObjectPlacedInLotEventArgs;
            var gameObject = args.ObjectId.ObjectFromId<GameObject>();
            if (gameObject == null)
                return;
            AddInteractions(gameObject, GetInteractionInjectListForType(gameObject.GetType()));
        }

        static void OnWorldQuit(object sender, EventArgs e)
        {
            // Clean up.
            if (gSimInstantiatedEventListener != null)
            {
                EventTracker.RemoveListener(gSimInstantiatedEventListener);
                gSimInstantiatedEventListener = null;
            }
        }

        /// <summary>
        /// Get the hooked interactions list for a GameObject type. Returns null if there isn't one.
        /// </summary>
        /// <param name="type">GameObject type.</param>
        /// <returns>Interaction list, or null.</returns>
        static List<InteractionDefinition> GetInteractionInjectListForType(Type type)
        {
            if (gInteractionsToInjectPerGameObject.TryGetValue(type, out var listResult))
                return listResult;
            return null;
        }

        /// <summary>
        /// Get the hooked interactions list for a GameObject type, or make a new one if there isn't one.
        /// </summary>
        /// <param name="type">GameObject type.</param>
        /// <returns>Interaction list.</returns>
        static List<InteractionDefinition> MakeInteractionInjectListForType(Type type)
        {
            if (gInteractionsToInjectPerGameObject.TryGetValue(type, out var listResult))
                return listResult;
            listResult = new List<InteractionDefinition>();
            gInteractionsToInjectPerGameObject[type] = listResult;
            return listResult;
        }

        static ListenerAction OnSimInstantiated(Event e)
        {
            if (!(e.TargetObject is GameObject gameObject))
                return ListenerAction.Keep;
            AddInteractions(gameObject, GetInteractionInjectListForType(e.TargetObject.GetType()));
            return ListenerAction.Keep;
        }

        /// <summary>
        /// Appends interactions into a GameObject instance, provided they're not already in the interaction list.
        /// </summary>
        /// <param name="gameObject">GameObject to inject interactions into.</param>
        /// <param name="interactions">Interaction list to inject.</param>
        static void AddInteractions(GameObject gameObject, List<InteractionDefinition> interactions)
        {
            if (interactions == null)
                return;
            foreach (var interaction in interactions)
            {
                foreach (var pair in gameObject.Interactions)
                {
                    if (pair.InteractionDefinition.GetType() == interaction.GetType())
                    {
                        return;
                    }
                }
                gameObject.AddInteraction(interaction);
            }
        }
    }
}
