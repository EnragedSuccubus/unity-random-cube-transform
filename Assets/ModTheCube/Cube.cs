using System.Collections;
using UnityEngine;

/// <summary>
/// Drive a cube's transform and material properties, with option continuous randomization over time using the same
/// ranges as the initial randomization. 
/// </summary>
/// <remarks>
/// - When <see cref="_randomize"/> is true, values are randomized at Start and then smoothly transition
///   to new random target forever.
/// - Transition speed is controlled by <see cref="_transitionDuration"/>.
/// - Rotation is applied every frame using the current <see cref="_rotationSpeed"/>.
/// </remarks>
public class Cube : MonoBehaviour
{
    /// <summary>
    /// Used to access the instance material for color updates.
    /// </summary>
    public MeshRenderer Renderer;
    
    // Serialized fields for inspector editing of the game object's properties.
    [SerializeField] private Vector3 _position = new(3, 4, 1);
    [SerializeField] private Vector3 _scale = Vector3.one * 1.3f;
    [SerializeField] private Color _color = new(0.5f, 1.0f, 0.3f, 0.4f);
    [SerializeField] private Vector3 _rotationSpeed = new(10.0f, 0.0f, 0.0f);
    [SerializeField] private bool _randomize;
    [SerializeField] private float _transitionDuration = 2.0f;

    /// <summary>
    /// Cached instance of the material so we can safely change color at runtime without modifying the shared material.
    /// </summary>
    private Material _materialInstance;

    /// <summary>
    /// Unity lifecycle: called before the first frame update.
    /// Initializes transform and material from the serialized fields, optionally randomizes, and starts
    /// continuous randomization if enabled.
    /// </summary>
    private void Start()
    {
        // Randomize the GameObject's properties if enabled.
        if (_randomize)
        {
            _position = RandomPosition();
            _scale = RandomScale();
            _color = RandomColor();
            _rotationSpeed = RandomRotation();
        }

        // Apply the transform values.
        transform.position = _position;
        transform.localScale = _scale;
 
        // Get a unique material instance so we can animate color per-object.
        Material materialInstance = Renderer.material;
        materialInstance.color = _color;

        // Cache the instance for repeated use to avoid property lookups.
        _materialInstance = materialInstance;
        
        // Begin the coroutine to randomize over time if enabled.
        if (_randomize)
        {
            StartCoroutine(RandomizeOverTime());
        }
    }

    /// <summary>
    /// Unity lifecycle: called once per frame.
    /// Applies rotation based on the current rotation speed and frame delta time.
    /// </summary>
    private void Update()
    {
        transform.Rotate(_rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Returns a random position based on a set of ranges. The ranges ensure the GameObject is always
    /// visible within the camera's visual boundary. 
    /// </summary>
    private Vector3 RandomPosition()
    {
        return new Vector3(
            Random.Range(0, 3), 
            Random.Range(-3, 6), 
            Random.Range(-8, 12)
        );
    }

    /// <summary>
    /// Returns a random uniform scale per-axis using a set of ranges.
    /// </summary>
    private Vector3 RandomScale()
    {
        return new Vector3(
            Random.Range(0.5f, 1.5f),
            Random.Range(0.5f, 1.5f), 
            Random.Range(0.5f, 1.5f)
        );
    }

    /// <summary>
    /// Returns a random RGBA color using a set of ranges.
    /// </summary>
    private Color RandomColor()
    {
        return new Color(
            Random.Range(0.0f, 1.0f),
            Random.Range(0.0f, 1.0f),
            Random.Range(0.0f, 1.0f),
            Random.Range(0.0f, 1.0f)
        );
    }

    /// <summary>
    /// Returns a random rotation speed (degrees per second) per-axis using a set of ranges.
    /// </summary>
    private Vector3 RandomRotation()
    {
        return new Vector3(
            Random.Range(-50.0f, 50.0f), 
            Random.Range(-50.0f, 50.0f),
            Random.Range(-50.0f, 50.0f)
        );
    }

    /// <summary>
    /// Continuously picks new random targets (position, scale, color, rotation speed) and smoothly
    /// interpolates current values toward those targets over <see cref="_transitionDuration"/> seconds.
    /// Uses the same random ranges as the initial setup.
    /// </summary>
    /// <returns>Coroutine enumerator.</returns>
    private IEnumerator RandomizeOverTime()
    {
        // Ensure we have a valid material instance to animate.
        Material material = _materialInstance;
        if (material == null) yield break;
        
        while (true)
        {
            // Capture current values as the start state for this transition.
            Vector3 startPosition = _position;
            Vector3 startScale = _scale;
            Color startColor = _color;
            Vector3 startRotationSpeed = _rotationSpeed;
            
            // Choose new targets using the same ranges as the initial setup.
            Vector3 targetPosition = RandomPosition();
            Vector3 targetScale = RandomScale();
            Color targetColor = RandomColor();
            Vector3 targetRotationSpeed = RandomRotation();

            // Elaspsed time in seconds since this transition began.
            float elapsedSeconds = 0f;
            
            // Interpolate from the start values to the target over the configurated duration.
            while (elapsedSeconds < _transitionDuration)
            {
                // Normalize progress in [0, 1] range.. If duration is 0 or less, jump to the target.
                float progress = _transitionDuration > 0f ? elapsedSeconds / _transitionDuration : 1f;
                
                // Apply a smooth step for ease-in/ease-out. Replace with 'progress' for linear.
                progress = Mathf.SmoothStep(0f, 1f, progress);

                // Learp the properties toward their targets.
                _position = Vector3.LerpUnclamped(startPosition, targetPosition, progress);
                _scale = Vector3.LerpUnclamped(startScale, targetScale, progress);
                _color = Color.LerpUnclamped(startColor, targetColor, progress);
                _rotationSpeed = Vector3.LerpUnclamped(startRotationSpeed, targetRotationSpeed, progress);
                
                transform.position = _position;
                transform.localScale = _scale;
                
                // Advance time and wait for the next frame.
                elapsedSeconds += Time.deltaTime;
                yield return null;
            }
            
            // Ensure the final state matches the chosen target to prevent drift.
            _position = targetPosition;
            _scale = targetScale;
            _color = targetColor;
            _rotationSpeed = targetRotationSpeed;
            
            transform.position = _position;
            transform.localScale = _scale;
            material.color = _color;
        }
    }
}