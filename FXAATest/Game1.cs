/*
 * Quick and dirty port of this sample
 * 
 * https://mtnphil.wordpress.com/2012/10/15/fxaa-in-xna/
 * 
 * to MonoGame. The FXAA shader has been writen by Timothy Lottes, more details here: * 
 * http://developer.download.nvidia.com/assets/gamedev/files/sdk/11/FXAA_WhitePaper.pdf
 * 
 * Use
 * 
 * - "F" to toggle between using FXAA and not using FXAA
 * - "G" to toggle between "console" and "pc quality" FXAA functions inside the shader
 * 
 */


#region USING STATEMENTS

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System.IO;
using System.Diagnostics;

#endregion

namespace FXAATest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region FIELDS, INIT, LOAD

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Effect fxaaEffect;
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;
        private RenderTarget2D renderTarget;
        private Texture2D testImage;
        private bool useFXAA;               // toggle FXAA
        private bool useQuality;            // toggle between "console" and "pc quality" modes

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 700;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            fxaaEffect = LoadEffect(@"../../../../fxaa.xnb", GraphicsDevice);
            testImage = LoadTexture(@"../../../../TestImage.jpg", GraphicsDevice);
        }

        // Loads a given texture from file
        public static Texture2D LoadTexture(string path, GraphicsDevice gd)
        {
            // check if file exists
            if (!File.Exists(path) || path == null)
            {
                Debug.WriteLine("LoadTexture: File <" + path + "> not found.");
                return null;
            }

            // load via filestream
            Texture2D tex;
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                tex = Texture2D.FromStream(gd, fs);
                fs.Close();
            }

            return tex;
        }

        // Loads a given effect from file (*.xnb)
        public static Effect LoadEffect(string path, GraphicsDevice gd)
        {
            // check if file exists
            if (!File.Exists(path) || path == null)
            {
                Debug.WriteLine("LoadEffect: File <" + path + "> not found.");
                return null;
            }

            Effect effect;
            using (BinaryReader b = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                effect = new Effect(gd, b.ReadBytes((int)b.BaseStream.Length));
            }

            return effect;
        }

        #endregion

        #region UNLOAD

        protected override void UnloadContent()
        {
            renderTarget.Dispose();
        }

        #endregion

        #region UPDATE

        private bool IsKeyPress(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            currentKeyboardState = Keyboard.GetState();

            if (IsKeyPress(Keys.F))
            {
                useFXAA = !useFXAA;
            }

            if (IsKeyPress(Keys.G))
            {
                useQuality = !useQuality;
            }

            previousKeyboardState = currentKeyboardState;

            base.Update(gameTime);
        }

        #endregion

        #region FXAA SETTINGS

        private float consoleEdgeSharpness = 8.0f;
        private float consoleEdgeThreshold = 0.125f;
        private float consoleEdgeThresholdMin = 0.05f;
        private float fxaaQualitySubpix = 0.75f;
        private float fxaaQualityEdgeThreshold = 0.166f;
        private float fxaaQualityEdgeThresholdMin = 0.0833f;
        
        #endregion

        #region DRAW

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            spriteBatch.Begin();
            spriteBatch.Draw(testImage, Vector2.Zero, Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            if (useFXAA)
            {
                float w = renderTarget.Width;
                float h = renderTarget.Height;

                if (!useQuality)
                {
                    fxaaEffect.CurrentTechnique = fxaaEffect.Techniques["ppfxaa_Console"];
                    fxaaEffect.Parameters["ConsoleOpt1"].SetValue(new Vector4(-2.0f / w, -2.0f / h, 2.0f / w, 2.0f / h));
                    fxaaEffect.Parameters["ConsoleOpt2"].SetValue(new Vector4(8.0f / w, 8.0f / h, -4.0f / w, -4.0f / h));
                    fxaaEffect.Parameters["ConsoleEdgeSharpness"].SetValue(consoleEdgeSharpness);
                    fxaaEffect.Parameters["ConsoleEdgeThreshold"].SetValue(consoleEdgeThreshold);
                    fxaaEffect.Parameters["ConsoleEdgeThresholdMin"].SetValue(consoleEdgeThresholdMin);
                }
                else
                {
                    fxaaEffect.CurrentTechnique = fxaaEffect.Techniques["ppfxaa_PC"];
                    fxaaEffect.Parameters["fxaaQualitySubpix"].SetValue(fxaaQualitySubpix);
                    fxaaEffect.Parameters["fxaaQualityEdgeThreshold"].SetValue(fxaaQualityEdgeThreshold);
                    fxaaEffect.Parameters["fxaaQualityEdgeThresholdMin"].SetValue(fxaaQualityEdgeThresholdMin);
                }

                fxaaEffect.Parameters["invViewportWidth"].SetValue(1f / w);
                fxaaEffect.Parameters["invViewportHeight"].SetValue(1f / h);
                fxaaEffect.Parameters["texScreen"].SetValue((Texture2D)renderTarget);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default,RasterizerState.CullNone, fxaaEffect);
                spriteBatch.Draw((Texture2D)renderTarget, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.White);
                spriteBatch.End();
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                spriteBatch.Draw((Texture2D)renderTarget, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        #endregion

    }
}
