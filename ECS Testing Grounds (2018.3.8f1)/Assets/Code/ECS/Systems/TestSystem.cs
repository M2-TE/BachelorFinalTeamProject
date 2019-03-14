using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class TestSystem : JobComponentSystem
{
	private struct TestJob : IJobProcessComponentData<Test>
	{
		public float Variable;
		public void Execute([ReadOnly] ref Test test)
		{
			Variable = test.Value;
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new TestJob()
		{
			Variable = 5f
		};

		return job.Schedule(this, inputDeps);
	}
}
