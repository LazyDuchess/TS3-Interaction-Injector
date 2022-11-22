using Sims3.SimIFace;

namespace LazyDuchess.InteractionInjection
{
	public class Main
	{
		[Tunable] static bool init;
		
		static Main()
		{
			InteractionInjector.Initialize();
		}
	}
}