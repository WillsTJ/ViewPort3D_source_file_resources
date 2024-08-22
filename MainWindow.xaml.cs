using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SlimDX;
using _3DTools;

namespace Morph_CE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Stores the left margin value for the crop tool, to remember as an anchor.
        /// </summary>
        private double cropToolMarginLeftBuffer = 0;
        
        /// <summary>
        /// Stores the top margin value for the crop tool, to remember as an anchor.
        /// </summary>
        private double cropToolMarginTopBuffer = 0;

        /// <summary>
        /// The visual for the cropping tool.
        /// </summary>
        private Rectangle cropRectangle = new Rectangle();

        /// <summary>
        /// When true, signifies that the mouse button left has been held down over the scene grid.
        /// </summary>
        private bool mouseLeftDownOverSceneGrid = false;

        /// <summary>
        /// Signifies that a visual is being resized and so it needs to be anchored.
        /// </summary>
        private bool resizeAnchor = false;

        /// <summary>
        /// Represents the axis where the model rotates.
        /// </summary>
        public double xaxisRotateAngle = 0;

        /// <summary>
        /// The angle about the x-plane of a mesh in the view port.
        /// </summary>
        public System.Windows.Media.Media3D.AxisAngleRotation3D rotationXaxisInDegrees;

        /// <summary>
        /// The angle about the y-plane of a mesh in the view port.
        /// </summary>
        public System.Windows.Media.Media3D.AxisAngleRotation3D rotationYaxisInDegrees;

        /// <summary>
        /// A reference to the rotate transform applied to a mesh.
        /// </summary>
        public System.Windows.Media.Media3D.RotateTransform3D rotateTransform3D = new System.Windows.Media.Media3D.RotateTransform3D();

        /// <summary>
        /// A reference to the directional light in the view.
        /// </summary>
        public System.Windows.Media.Media3D.DirectionalLight myDirectionalLight = new System.Windows.Media.Media3D.DirectionalLight();

        /// <summary>
        /// Store the X-axis value of the mouse pointer, relative to the ViewPort3D
        /// canvas space.
        /// </summary>
        Point mousePointerCoordinate;

        /// <summary>
        /// Declare where the directional light of the mesh is aiming respective of the x-plane.
        /// </summary>
        private double directionalLightXPosition;        
        
        /// <summary>
        /// Declare where the directional light of the mesh is aiming respective of the y-plane.
        /// </summary>
        private double directionalLightYPosition;

        /// <summary>
        /// A 3D Point used to track the diffused light position within the main ViewPort3D.
        /// </summary>
        public System.Windows.Media.Media3D.Point3D diffuseLightPoint3D;

        public MainWindow()
        {
            InitializeComponent();

            // Instantiate x-axis rotation vector property reference.
            this.rotationXaxisInDegrees = new System.Windows.Media.Media3D.AxisAngleRotation3D();

            // Instantiate ViewPort3D resources.
            this.mousePointerCoordinate = new Point();

           _3DTools.Trackport3D trackport = new Trackport3D();
        }

        /// <summary>
        /// A public accessor to the X-position of the directional light.
        /// </summary>
        public double DirectionalLightXPosition
        {
            get
            {
                return this.directionalLightXPosition;
            }
            set
            {
                this.directionalLightXPosition = value;
            }
        }        
        
        /// <summary>
        /// A public accessor to the Y-position of the directional light.
        /// </summary>
        public double DirectionalLightYPosition
        {
            get
            {
                return this.directionalLightYPosition;
            }
            set
            {
                this.directionalLightYPosition = value;
            }
        }

        /// <summary>
        /// Opens an image file and places it on the scene.
        /// </summary>
        private void OpenFileImage()
        {
           // this.ViewPortImage.Source = ViewEncoder.OpenImage();
        }

        /// <summary>
        /// Track the relative coordinates of the mouse movements
        /// around the ViewPort3D X,Y plane.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainViewPort_MouseMove(object sender, MouseEventArgs e)
        {
            if (ButtonStatePipeline.MouseButtonLeft == MouseButtonState.Pressed)
            {
                System.Windows.Media.Media3D.Point3DCollection point3DCOllection = new System.Windows.Media.Media3D.Point3DCollection();
                point3DCOllection.Add(new System.Windows.Media.Media3D.Point3D(e.GetPosition(this.MainViewPort).X, e.GetPosition(this.MainViewPort).X, 0));
                this.BackgroundImageMeshGeometry3D.Positions = point3DCOllection;
            }

            // Update the light source directions.
            this.UpdateLightValues(e);

            // Return the position of the mouse pointer, relative to the ViewPort3D.
            this.mousePointerCoordinate = e.GetPosition(this.MainViewPort);
            this.UpdateCrosshair(this.mousePointerCoordinate);
        }

        /// <summary>
        /// Update the crosshair position to trail with the mouse pointer.
        /// </summary>
        /// <param name="mousePosition">Takes a position of the mouse to reference for the crosshair position.</param>
        private void UpdateCrosshair(Point mousePosition)
        {

            // Update the light source gizmo graphic position within the ViewPort3D canvas.
            this.LightSourceGizmoEllipse.Margin = new Thickness(mousePosition.X, mousePosition.Y ,0 ,0);
        }

        /// <summary>
        /// Update the light values of the types of light on the objects in the viewport.
        /// </summary>
        private void UpdateLightValues(MouseEventArgs e)
        {
            // Return the diffuse lighting position according to the light source gizmo position,
            // against the mesh of the 3D mesh in the MainViewPort3D
            this.diffuseLightPoint3D = new System.Windows.Media.Media3D.Point3D(this.mousePointerCoordinate.X + 90, this.mousePointerCoordinate.Y + 90, 0);
            this.MainDirectionalLight.Direction = (System.Windows.Media.Media3D.Vector3D)diffuseLightPoint3D;

            // Return the position of the mouse pointer, relative to the ViewPort3D.
            this.mousePointerCoordinate = e.GetPosition(this.MainViewPort);
            this.UpdateCrosshair(this.mousePointerCoordinate);

            this.MainViewPort.InvalidateVisual();
        }

        private void MainDirectionalLight_Changed(object sender, EventArgs e)
        {
            
        }

        private void MainViewPort_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Update the light source directions.
            this.UpdateLightValues(e);

            // Rotate the mesh.
            this.RotateMeshX(this.MainAxisAngleRotation3D, e); // In future, this should be dynamically handled.
        }

        /// <summary>
        /// Rotates the ViewPort3D mesh about the x-axis.
        /// </summary>
        /// <param name="e"></param>
        private void RotateMeshX(System.Windows.Media.Media3D.AxisAngleRotation3D axisAngleRotation3D, MouseWheelEventArgs e)
        {
            if (axisAngleRotation3D.Equals(this.MainViewPort))
            {
                axisAngleRotation3D = this.MainAxisAngleRotation3D;
            }

            //if (axisAngleRotation3D.Equals(this.ViewPortImage))
            //{
            //    axisAngleRotation3D = this.ViewPortImageAxisAngleRotation3D;
            //}

            if (e.Delta == 120 && ButtonStatePipeline.MouseButtonRight == MouseButtonState.Pressed)
            {
                // this.MainAxisAngleRotation3D.Angle.
                axisAngleRotation3D.Axis = new System.Windows.Media.Media3D.Vector3D(0, 1, 0);
                xaxisRotateAngle = xaxisRotateAngle += 2 % 360;
                axisAngleRotation3D.Angle = xaxisRotateAngle;
            }
            else if (e.Delta == -120 && ButtonStatePipeline.MouseButtonRight == MouseButtonState.Pressed)
            {
                axisAngleRotation3D.Axis = new System.Windows.Media.Media3D.Vector3D(0, 1, 0);
                xaxisRotateAngle = xaxisRotateAngle -= 2 % 360;
                axisAngleRotation3D.Angle = xaxisRotateAngle;
            }
            else if (e.Delta == 0)
            {
                axisAngleRotation3D = rotationXaxisInDegrees;
            }

            if (e.Delta == 120 && ButtonStatePipeline.MouseButtonLeft == MouseButtonState.Pressed)
            {
                axisAngleRotation3D.Axis = new System.Windows.Media.Media3D.Vector3D(1, 0, 0);
                xaxisRotateAngle = xaxisRotateAngle += 2 % 360;
                axisAngleRotation3D.Angle = xaxisRotateAngle;
            }
            else if (e.Delta == -120 && ButtonStatePipeline.MouseButtonLeft == MouseButtonState.Pressed)
            {
                axisAngleRotation3D.Axis = new System.Windows.Media.Media3D.Vector3D(1, 0, 0);
                xaxisRotateAngle = xaxisRotateAngle -= 2 % 360;
                axisAngleRotation3D.Angle = xaxisRotateAngle;
            }
            else if (e.Delta == 0)
            {
                axisAngleRotation3D = rotationXaxisInDegrees;
            }
        }

        /// <summary>
        /// When the ViewPort3D has been clicked by either left or right mouse
        /// button input, pass the details to an intermediary class which stores
        /// the state information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainViewPort_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Pass the mouse state to have it's state processed.
            ButtonStatePipeline.SetButtonState(e);
        }

        /// <summary>
        /// When the ViewPort3D has been clicked by either left or right mouse
        /// button input, pass the details to an intermediary class which stores
        /// the state information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainViewPort_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Pass the mouse state to have it's state processed.
            ButtonStatePipeline.SetButtonState(e);
        }

        /// <summary>
        /// Opens an image file and places it onto the scene.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportImageButton_Click(object sender, RoutedEventArgs e)
        {
            this.OpenFileImage();
        }

        /// <summary>
        /// Enables the crop tool to initialize the crop automation properties.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CropToolButon_Click(object sender, RoutedEventArgs e)
        {
            // Create UI visual for the crop region.
            cropRectangle.Height = 10;
            cropRectangle.Width = 10;
            cropRectangle.Fill = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
            cropRectangle.Stroke = new SolidColorBrush(System.Windows.Media.Colors.Black);
            cropRectangle.StrokeThickness = 1.5;

            // Add the crop visual to the scene grid.
            // This unifies the dimensions for the crop visual and image containers
            // in the scene.
            this.MainSceneGrid.Children.Add(cropRectangle);

            this.MainSceneGrid.MouseMove += new MouseEventHandler(this.MainSceneGrid_MouseMove);
            this.MainSceneGrid.MouseLeftButtonDown += new MouseButtonEventHandler(this.ResizeCropTool_MouseDown);
            this.MainSceneGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.ResizeCropTool_MouseUp);
        }

        /// <summary>
        /// Fires a hit test when the mouse is over the grid terrain.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainSceneGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (ButtonStatePipeline.MouseButtonLeft == MouseButtonState.Pressed)
            {
                // Move the crop tool about the scene grid.
                if (this.resizeAnchor == false)
                {
                    this.cropRectangle.Margin = new Thickness(e.GetPosition(this.MainSceneGrid).X, e.GetPosition(this.MainSceneGrid).Y, 0, 0);
                }

                // Don't move the rectangle whilst resizing the crop range.
                if (this.resizeAnchor == true)
                {
                    this.cropRectangle.Height = Math.Abs(cropToolMarginTopBuffer - e.GetPosition(this.MainSceneGrid).Y);
                    this.cropRectangle.Width = Math.Abs(cropToolMarginLeftBuffer - e.GetPosition(this.MainSceneGrid).X);
                }
            }
            else if (ButtonStatePipeline.MouseButtonLeft == MouseButtonState.Released)
            {
                if (this.resizeAnchor == false)
                {
                    // Store the crop tool top and left margins, in order to anchor the control visual.
                    this.resizeAnchor = true;
                    this.cropToolMarginLeftBuffer = this.cropRectangle.Margin.Left;
                    this.cropToolMarginTopBuffer = this.cropRectangle.Margin.Top;
                }
            }
        }

        /// <summary>
        /// Toggle a bool that signifies the mouse down state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeCropTool_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonStatePipeline.SetButtonState(e);
        }

        /// <summary>
        /// Toggle a bool that signifies the mouse down state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeCropTool_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Update the button state reference.
            ButtonStatePipeline.SetButtonState(e);

            // Update the bitmap contained in the main image container.
            this.MainImage.Source = ViewEncoder.CropImage(ViewEncoder.BitmapImageStored, (int)this.cropToolMarginLeftBuffer, (int)this.cropToolMarginTopBuffer, (int)this.cropRectangle.Width, (int)this.cropRectangle.Height);
        }

        private void AddCubeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ViewPortImage_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void ViewPortImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Pass the mouse state to have it's state processed.
            ButtonStatePipeline.SetButtonState(e);

            // Rotate the mesh.
            this.RotateMeshX(this.ViewPortImageAxisAngleRotation3D, e);
        }

        private void ViewPortImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Pass the mouse state to have it's state processed.
            ButtonStatePipeline.SetButtonState(e);
        }

        private void ViewPortImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Pass the mouse state to have it's state processed.
            ButtonStatePipeline.SetButtonState(e);
        }

        private void ApplyTextureToGeometryButton_Click(object sender, RoutedEventArgs e)
        {
            ImageSource imageSource = ViewEncoder.OpenImage();

            DrawingBrush BigPlanetBrush = new DrawingBrush(new ImageDrawing(imageSource, new Rect(0, 0, 600, 600)));

            BigPlanetBrush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            BigPlanetBrush.ViewboxUnits = BrushMappingMode.Absolute;
            BigPlanetBrush.TileMode = TileMode.Tile;
            BigPlanetBrush.Stretch = Stretch.Fill;

            RenderOptions.SetCachingHint(BigPlanetBrush, CachingHint.Cache);
            RenderOptions.SetCacheInvalidationThresholdMinimum(BigPlanetBrush, 0.5);
            RenderOptions.SetCacheInvalidationThresholdMaximum(BigPlanetBrush, 2.0);

            this.BPDiffuseMaterial = new System.Windows.Media.Media3D.DiffuseMaterial(BigPlanetBrush);  // DrawingBrush();
            this.BPDiffuseMaterial.Color = System.Windows.Media.Colors.White;
            BPGeometryMaterial.BackMaterial = this.BPDiffuseMaterial;
            this.Viewport1.InvalidateVisual(); 
        }

        private void Viewport1_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void Viewport1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Pass the mouse state to have it's state processed.
            ButtonStatePipeline.SetButtonState(e);

            // Rotate the mesh.
            this.RotateMeshX(this.PlanetAxisAngleRotation3D, e);

            if (ButtonStatePipeline.MouseButtonLeft == MouseButtonState.Released)
            {
                if (e.Delta == 120)
                {
                    this.myCamera.FieldOfView = this.myCamera.FieldOfView += 4;
                }

                if (e.Delta == -120)
                {
                    this.myCamera.FieldOfView = this.myCamera.FieldOfView -= 4;
                }
            }
        }

        /// <summary>
        /// Determine the field-of-view for the VP2, when the mouse wheel is being scrolled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewPort2_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ButtonStatePipeline.MouseButtonLeft == MouseButtonState.Released)
            {
                if (e.Delta == 120)
                {
                    this.VP2FieldOfView.FieldOfView -= 0.8;
                }
                else if (e.Delta == -120)
                {
                    this.VP2FieldOfView.FieldOfView += 0.8;
                }
           
                if (e.Delta == 120)
                {
                    this.rotate.Axis = new System.Windows.Media.Media3D.Vector3D(1, 0, 0);
                    xaxisRotateAngle = xaxisRotateAngle += 2 % 360;
                    this.rotate.Angle = xaxisRotateAngle;
                }
                else if (e.Delta == -120)
                {
                    this.rotate.Axis = new System.Windows.Media.Media3D.Vector3D(1, 0, 0);
                    xaxisRotateAngle = xaxisRotateAngle -= 2 % 360;
                    this.rotate.Angle = xaxisRotateAngle;
                }
            }
        }
    }
}