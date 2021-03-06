using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Vehicle;
using BEPUphysicsDrawer.Models;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.CollisionShapes;
using BEPUutilities;
using ConversionHelper;
using System.Diagnostics;
using FixMath.NET;

namespace BEPUphysicsDemos.AlternateMovement
{
    /// <summary>
    /// Handles input and movement of a Vehicle in the game.
    /// Acts as the 'front end' for the bookkeeping and math of the Vehicle within the physics engine.
    /// </summary>
    public class VehicleInput
    {
        /// <summary>
        /// Speed that the Vehicle tries towreach when moving backward.
        /// </summary>
        public Fix64 BackwardSpeed = -13;

        /// <summary>
        /// Speed that the Vehicle tries to reach when moving forward.
        /// </summary>
        public Fix64 ForwardSpeed = 30;

        /// <summary>
        /// Whether or not to use the Vehicle's input.
        /// </summary>
        public bool IsActive;


        /// <summary>
        /// Maximum turn angle of the wheels.
        /// </summary>
        public Fix64 MaximumTurnAngle = MathHelper.Pi / 6;

        /// <summary>
        /// Draws the body and wheels.
        /// </summary>
        public ModelDrawer ModelDrawer;

        /// <summary>
        /// Owning space of the Vehicle.
        /// </summary>
        public Space Space;

        /// <summary>
        /// Turning speed of the wheels in radians per second.
        /// </summary>
        public Fix64 TurnSpeed = MathHelper.Pi;

        /// <summary>
        /// Physics representation of the Vehicle.
        /// </summary>
        public Vehicle Vehicle;

        /// <summary>
        /// List of graphical representations of the wheels on the Vehicle.
        /// </summary>
        public List<DisplayModel> WheelModels;

        /// <summary>
        /// Gets the camera control scheme ued by this input manager.
        /// </summary>
        public ChaseCameraControlScheme CameraControlScheme { get; private set; }


        /// <summary>
        /// Constructs the front end and the internal physics representation of the Vehicle.
        /// </summary>
        /// <param name="position">Position of the Vehicle.</param>
        /// <param name="space">Space to add the Vehicle to.</param>
        /// <param name="camera">Camera to attach to the Vehicle.</param>
        /// <param name="game">The running game.</param>
        /// <param name="drawer">Drawer used to draw the Vehicle.</param>
        /// <param name="wheelModel">Model of the wheels.</param>
        /// <param name="wheelTexture">Texture to use for the wheels.</param>
        public VehicleInput(Vector3 position, Space space, Camera camera, DemosGame game, ModelDrawer drawer, Model wheelModel, Texture2D wheelTexture)
        {
            var bodies = new List<CompoundShapeEntry>
                {
                    new CompoundShapeEntry(new BoxShape((Fix64)2.5m, (Fix64).75m, (Fix64)4.5m), new Vector3(0, 0, 0), 60),
                    new CompoundShapeEntry(new BoxShape((Fix64)2.5m, (Fix64).3m, (Fix64)2f), new Vector3(0, (Fix64).75m / 2 + (Fix64).3m / 2, (Fix64).5m), 1)
                };
            var body = new CompoundBody(bodies, 61);
            body.CollisionInformation.LocalPosition = new Vector3(0, (Fix64).5m, 0);
            body.Position = position; //At first, just keep it out of the way.
            Vehicle = new Vehicle(body);

            var localWheelRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), MathHelper.PiOver2);

            //The wheel model used is not aligned initially with how a wheel would normally look, so rotate them.
            Matrix wheelGraphicRotation = Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.PiOver2);
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape((Fix64).375m, (Fix64)0.2m, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(2000, 100, Vector3.Down, (Fix64)0.325m, new Vector3((Fix64)(- 1.1m), (Fix64)(-0.1m), (Fix64)1.8m)),
                                 new WheelDrivingMotor((Fix64)2.5m, 30000, 10000),
                                 new WheelBrake((Fix64)1.5m, 2, (Fix64).02m),
                                 new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape((Fix64).375m, (Fix64)0.2m, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(2000, 100, Vector3.Down, (Fix64)0.325m, new Vector3((Fix64)(-1.1m), (Fix64)(- 0.1m), (Fix64)(- 1.8m))),
                                 new WheelDrivingMotor((Fix64)2.5m, 30000, 10000),
                                 new WheelBrake((Fix64)1.5m, 2, (Fix64).02m),
                                 new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape((Fix64).375m, (Fix64)0.2m, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(2000, 100, Vector3.Down, (Fix64)0.325m, new Vector3((Fix64)1.1m, (Fix64)(-0.1m), (Fix64)1.8m)),
                                 new WheelDrivingMotor((Fix64)2.5m, 30000, 10000),
                                 new WheelBrake((Fix64)1.5m, 2, (Fix64).02m),
                                 new WheelSlidingFriction(4, 5)));
            Vehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape((Fix64).375m, (Fix64)0.2m, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(2000, 100, Vector3.Down, (Fix64)0.325m, new Vector3((Fix64)1.1m, (Fix64)(-0.1m), (Fix64)(-1.8m))),
                                 new WheelDrivingMotor((Fix64)2.5m, 30000, 10000),
                                 new WheelBrake((Fix64)1.5m, 2, (Fix64).02m),
                                 new WheelSlidingFriction(4, 5)));


            foreach (Wheel wheel in Vehicle.Wheels)
            {
                //This is a cosmetic setting that makes it looks like the car doesn't have antilock brakes.
                wheel.Shape.FreezeWheelsWhileBraking = true;

                //By default, wheels use as many iterations as the space.  By lowering it,
                //performance can be improved at the cost of a little accuracy.
                //However, because the suspension and friction are not really rigid,
                //the lowered accuracy is not so much of a problem.
                wheel.Suspension.SolverSettings.MaximumIterationCount = 1;
                wheel.Brake.SolverSettings.MaximumIterationCount = 1;
                wheel.SlidingFriction.SolverSettings.MaximumIterationCount = 1;
                wheel.DrivingMotor.SolverSettings.MaximumIterationCount = 1;
            }

            Space = space;

            Space.Add(Vehicle);
            ModelDrawer = drawer;
            DisplayModel model;
            WheelModels = new List<DisplayModel>();
            for (int k = 0; k < 4; k++)
            {
                Vehicle.Wheels[k].Shape.Detector.Tag = "noDisplayObject";
                model = new DisplayModel(wheelModel, ModelDrawer);
                ModelDrawer.Add(model);
                WheelModels.Add(model);
                model.Texture = wheelTexture;
            }



            CameraControlScheme = new ChaseCameraControlScheme(Vehicle.Body, new Vector3(0, (Fix64)0.6m, 0), true, 10, camera, game);

        }

        /// <summary>
        /// Gives the Vehicle control over the camera and movement input.
        /// </summary>
        public void Activate(Vector3 position)
        {
            if (!IsActive)
            {
                IsActive = true;
                //Put the Vehicle where the camera is.
                Vehicle.Body.Position = position;
                Vehicle.Body.LinearVelocity = Vector3.Zero;
                Vehicle.Body.AngularVelocity = Vector3.Zero;
                Vehicle.Body.Orientation = Quaternion.Identity;
            }
        }

        /// <summary>
        /// Returns input control to the camera.
        /// </summary>
        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
            }
        }


        /// <summary>
        /// Handles the input and movement of the character.
        /// </summary>
        /// <param name="dt">Time since last frame in simulation seconds.</param>
        /// <param name="keyboardInput">Keyboard state.</param>
        /// <param name="gamePadInput">Gamepad state.</param>
        public void Update(Fix64 dt, KeyboardState keyboardInput, GamePadState gamePadInput)
        {
            //Update the wheel's graphics.
            for (int k = 0; k < 4; k++)
            {
                WheelModels[k].WorldTransform = Vehicle.Wheels[k].Shape.WorldTransform;
            }

            if (IsActive)
            {
                CameraControlScheme.Update(dt);
#if XBOX360
                Fix64 speed = gamePadInput.Triggers.Right * ForwardSpeed + gamePadInput.Triggers.Left * BackwardSpeed;
                Vehicle.Wheels[1].DrivingMotor.TargetSpeed = speed;
                Vehicle.Wheels[3].DrivingMotor.TargetSpeed = speed;

                if (gamePadInput.IsButtonDown(Buttons.LeftStick))
                    foreach (Wheel wheel in Vehicle.Wheels)
                    {
                        wheel.Brake.IsBraking = true;
                    }
                else
                    foreach (Wheel wheel in Vehicle.Wheels)
                    {
                        wheel.Brake.IsBraking = false;
                    }
                Vehicle.Wheels[1].Shape.SteeringAngle = (gamePadInput.ThumbSticks.Left.X * MaximumTurnAngle);
                Vehicle.Wheels[3].Shape.SteeringAngle = (gamePadInput.ThumbSticks.Left.X * MaximumTurnAngle);
#else

                if (keyboardInput.IsKeyDown(Keys.E))
                {
                    //Drive
                    Vehicle.Wheels[1].DrivingMotor.TargetSpeed = ForwardSpeed;
                    Vehicle.Wheels[3].DrivingMotor.TargetSpeed = ForwardSpeed;
                }
                else if (keyboardInput.IsKeyDown(Keys.D))
                {
                    //Reverse
                    Vehicle.Wheels[1].DrivingMotor.TargetSpeed = BackwardSpeed;
                    Vehicle.Wheels[3].DrivingMotor.TargetSpeed = BackwardSpeed;
                }
                else
                {
                    //Idle
                    Vehicle.Wheels[1].DrivingMotor.TargetSpeed = 0;
                    Vehicle.Wheels[3].DrivingMotor.TargetSpeed = 0;
                }
                if (keyboardInput.IsKeyDown(Keys.Space))
                {
                    //Brake
                    foreach (Wheel wheel in Vehicle.Wheels)
                    {
                        wheel.Brake.IsBraking = true;
                    }
                }
                else
                {
                    //Release brake
                    foreach (Wheel wheel in Vehicle.Wheels)
                    {
                        wheel.Brake.IsBraking = false;
                    }
                }
                //Use smooth steering; while held down, move towards maximum.
                //When not pressing any buttons, smoothly return to facing forward.
                Fix64 angle;
                bool steered = false;
                if (keyboardInput.IsKeyDown(Keys.S))
                {
                    steered = true;
                    angle = MathHelper.Max(Vehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * dt, -MaximumTurnAngle);
                    Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                }
                if (keyboardInput.IsKeyDown(Keys.F))
                {
                    steered = true;
                    angle = MathHelper.Min(Vehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * dt, MaximumTurnAngle);
                    Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                }
                if (!steered)
                {
                    //Neither key was pressed, so de-steer.
                    if (Vehicle.Wheels[1].Shape.SteeringAngle > 0)
                    {
                        angle = MathHelper.Max(Vehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * dt, 0);
                        Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                        Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                    }
                    else
                    {
                        angle = MathHelper.Min(Vehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * dt, 0);
                        Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                        Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                    }
                }


#endif
            }
            else
            {
                //Parking brake
                foreach (Wheel wheel in Vehicle.Wheels)
                {
                    wheel.Brake.IsBraking = true;
                }
                //Don't want the car to keep trying to drive.
                Vehicle.Wheels[1].DrivingMotor.TargetSpeed = 0;
                Vehicle.Wheels[3].DrivingMotor.TargetSpeed = 0;
            }
        }
    }
}