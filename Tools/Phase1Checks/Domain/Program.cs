using System;
using System.Reflection;
using NUnit.Framework;
class Program
{
    static int Main() {
        int passed=0,failed=0;
        foreach(var type in new[]{typeof(BoardGridTests),typeof(HelpPositionPlannerTests),typeof(BallStateTests),typeof(InputRangeTests),typeof(GyroLifecycleTests),typeof(GyroInputProviderTests),typeof(InputProviderFactoryTests),typeof(KeyboardInputProviderTests),typeof(TouchInputProviderTests)}) {
            var fixture=Activator.CreateInstance(type);
            foreach(var method in type.GetMethods()) {
                if(method.GetCustomAttribute<TestAttribute>()==null) continue;
                try { method.Invoke(fixture,null); Console.WriteLine("PASS "+type.Name+"."+method.Name);passed++; }
                catch(Exception e) { Console.WriteLine("FAIL "+method.Name+": "+(e.InnerException??e));failed++; }
            }
        }
        Console.WriteLine($"{passed} passed; {failed} failed (standalone .NET; no Unity runtime).");return failed==0?0:1;
    }
}
