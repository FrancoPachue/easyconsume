using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Generic;
using Orleans;
using Orleans.EventSourcing;
using Orleans.Runtime;
using EasyConsume.GrainInterfaces;
using System.Collections.Generic;
using Orleans.Providers;
using Orleans.Serialization.Cloning;

[LogConsistencyProvider(ProviderName = "StateStorage")]
public class EventGrain : JournaledGrain<EventState>, IEventGrain
{
    private readonly IPersistentState<EventState> _persistentState;
    
    public EventGrain([PersistentState("eventState", "MemoryStore")] IPersistentState<EventState> persistentState)
    {
        _persistentState = persistentState;
    }

    public Task Process(string message)
    {
        var eventData = new NewOdd { Name = "Franco", EventData = message };
        RaiseEvent(eventData); // Levanta el evento para actualizar el estado
        var events = GetConfirmedEvents().Result;
        if ( events.Count != 0)
        {
            Console.WriteLine($"{events.Count}");
        }
        return ConfirmEvents(); // Confirma los eventos para que se registren
    }

    public Task<List<string>> GetConfirmedEvents()
    {
        return Task.FromResult(State.ConfirmedEvents);
    }
}

// Define el estado del grano
[GenerateSerializer]
public class EventState
{
    public string Name { get; set; }
    public List<string> ConfirmedEvents { get; set; } = new List<string>();

    // Método para aplicar un evento al estado
    public void Apply(NewOdd @event)
    {
        Name = @event.Name;
        ConfirmedEvents.Add(@event.EventData);
    }
}

[RegisterCopier]
public class EventStateCopier : IDeepCopier<EventState>
{
    public EventState DeepCopy(EventState original, CopyContext context)
    {
        // Si ya hemos copiado este objeto en el contexto actual, devolvemos la copia existente
        if (context.TryGetCopy<EventState>(original, out var existingCopy))
        {
            return existingCopy;
        }

        // Crear una nueva instancia de EventState
        var copy = new EventState
        {
            Name = original.Name,
            ConfirmedEvents = new List<string>(original.ConfirmedEvents) // Copia profunda de la lista
        };

        // Registrar la nueva copia en el contexto
        context.RecordCopy(original, copy);

        return copy;
    }
}

// Define un evento que puede ser aplicado al grano
public class NewOdd
{
    public string Name { get; set; }
    public string EventData { get; set; }
}