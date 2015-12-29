using UnityEngine;
using System.Collections.Generic;

namespace DreamScape
{
	public class GameStateManager : MonoBehaviour
	{
		private static GameStateManager instance;
		private static IGameState currentState;
		private static Dictionary<System.Type, IGameState> states;

		public IGameState startingState;

		public static GameStateManager Instance
		{
			get { return instance ?? (new GameObject("GameTime Global")).AddComponent<GameStateManager>(); }
			private set { instance = value; }
		}

		void Awake()
		{
			if (instance == null)
				instance = this;

			else if (instance != this)
			{
				Destroy(this);
				return;
			}

			states = new Dictionary<System.Type, IGameState>();
			if (startingState != null)
			{
				states.Add(startingState.GetType(), startingState);
				currentState = startingState;
			}
		}

		public static void ChangeState<T>() where T : IGameState
		{
			currentState.Exit();
			currentState = states[typeof(T)];
			currentState.Enter();
		}

		public static void RegisterState<T>() where T : IGameState, new()
		{
			if (states.ContainsKey(typeof(T)))
				return;

			states.Add(typeof(T), new T());
		}

		// Update is called once per frame
		void Update()
		{
			if (currentState != null)
				currentState.Update();
		}
	}

	public interface IGameState
	{
		void Initialize();
		void Enter();
		void Exit();
		void Update();
	}

	public abstract class GameState : IGameState
	{
		public abstract void Initialize();
		public abstract void Enter();
		public abstract void Exit();
		public abstract void Update();
	}
}