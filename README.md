# TS3-Interaction-Injector
Pure script mod that allows the user to easily inject custom interactions into existing objects or Sims in The Sims 3.

## Example Usage
```
using Sims3.SimIFace;
using LazyDuchess.InteractionInjection;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;

namespace LazyDuchess.InjectionTest
{
	public class Main
	{
		[Tunable] static bool init;
		
		static Main()
		{
			// Add Chat interaction to all Sims
			InteractionInjector.RegisterInteraction<Sim>(Sim.Chat.Singleton);

			// Add Teleport Me Here interaction to floor and ground.
			InteractionInjector.RegisterInteraction<Terrain>(Terrain.TeleportMeHere.Singleton);
		}

		public static void RemoveInteractions()
        	{
			// Remove the interactions we injected in Main.
			InteractionInjector.UnregisterInteraction<Sim>(Sim.Chat.Singleton);
			
			InteractionInjector.UnregisterInteraction<Terrain>(Terrain.TeleportMeHere.Singleton);
		}
	}
}
```
