﻿using System;
using System.Runtime.InteropServices;
using System.Timers;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace YEET
{
    public static class StateMaschine
    {
        static StateMaschine()
        {
            Console.WriteLine("StateMaschine Started");
        }
        
        private class Startupstate : State
        {
            public Startupstate(State new_state)
            {
                _startState = new_state;
            }
            public override void OnUpdate(FrameEventArgs e)
            {
                Console.WriteLine("Startup created. Will switch to First State now");
                base.OnUpdate(e);
                StateMaschine.SwitchState(_startState);
            }

            private State _startState;
        }
        
        
        public static void Run(State start,int updateRate, int frameRate)
        {
            _currentState = new Startupstate(start);
            Context = new MainWindow(updateRate,frameRate);
            Context.Run();
        }
        
        public static void Render()
        {
            _currentState.OnRender();
            _currentState.OnGui();
        }

        public static void Input()
        {
            _currentState.OnInput();
        }
        
        public static void Update(FrameEventArgs e)
        {
            
            _currentState.OnUpdate(e);
        }
        
        public static void SwitchState(State nextState)
        {
            _currentState.OnLeave();
            _currentState = nextState;
            _currentState.OnStart();
        }

        public static void Exit()
        {
            _currentState.OnLeave();
            Context.Close();
            Context.Dispose();
        }
        
        

        private static State _currentState;
        public static MainWindow Context;
    }

    public class State
    {
        public State()
        {
            
        }
        
        /// <summary>
        /// Gets called after the render call. add all Imgui Code in this block
        /// </summary>
        public virtual void OnGui()
        {
            
        }

        public virtual void OnInput()
        {
            
        }
        
        /// <summary>
        /// Add all update functions in this block. It is Time-managed through the opentk game window
        /// </summary>
        public virtual void OnUpdate( FrameEventArgs e)
        {
            
        }
        /// <summary>
        /// all Rendercode should be here or you will run into gl.clear problems
        /// </summary>
        public virtual void OnRender()
        {
            
        }
        /// <summary>
        /// gets called once when the state gets loaded
        /// </summary>
        public virtual void OnStart()
        {
        }
        /// <summary>
        /// gets called once when the state gets switched away by the statemaschine
        /// </summary>
        public virtual void OnLeave()
        {
        }
    }

    
    

    
}