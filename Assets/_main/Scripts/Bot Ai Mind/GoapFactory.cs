using DependencyInjection;
using UnityEngine;
using UnityServiceLocator;

public class GoapFactory : MonoBehaviour, IDependencyProvider
{
    void Awake()
    {
        ServiceLocator.Global.Register(this);
    }

    [Provide] public GoapFactory ProvideFactory() => this;
    public IGoapPlanner CreatePlanner()
    {
        return new GoapPlanner();
    }
}