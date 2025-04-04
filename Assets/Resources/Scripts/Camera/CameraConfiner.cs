using UnityEngine;
using Cinemachine;

public class CameraConfiner : MonoBehaviour
{
    [Tooltip("El transform del jugador que la cámara seguirá")]
    [SerializeField] private Transform playerTransform;
    
    [Tooltip("El collider que define los límites del área")]
    [SerializeField] private Collider2D boundingShape;
    
    [Header("Ajustes de Suavizado")]
    [Tooltip("Velocidad de damping en X (valores más altos = movimiento más suave)")]
    [Range(0f, 5f)]
    [SerializeField] private float xDamping = 1f;
    
    [Tooltip("Velocidad de damping en Y (valores más altos = movimiento más suave)")]
    [Range(0f, 5f)]
    [SerializeField] private float yDamping = 1f;
    
    [Tooltip("Dead Zone - Área donde pequeños movimientos del target son ignorados")]
    [Range(0f, 1f)]
    [SerializeField] private float deadZoneWidth = 0.1f;
    [Range(0f, 1f)]
    [SerializeField] private float deadZoneHeight = 0.1f;
    
    [Header("Ajustes de Posición")]
    [SerializeField] private float cameraDistance = -10f;
    [SerializeField] private float orthographicSize = 5f;
    [SerializeField] private float offsetX = 0f;
    [SerializeField] private float offsetY = 0f;
    
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBrain brain;
    
    private void Start()
    {
        SetupCamera();
    }
    
    private void SetupCamera()
    {
        // Configurar la cámara principal
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No se encontró la Main Camera");
            return;
        }
        
        mainCamera.orthographic = true;
        
        // Configurar el CinemachineBrain para transiciones suaves
        brain = mainCamera.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            brain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
        }
        
        // Ajustes importantes para evitar saltos
        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        brain.m_DefaultBlend.m_Time = 2.0f;
        brain.m_CameraActivatedEvent?.RemoveAllListeners();
        
        // Crear la cámara virtual
        GameObject vcamObj = new GameObject("CM_SmoothFollowCam");
        virtualCamera = vcamObj.AddComponent<CinemachineVirtualCamera>();
        
        // Configuración básica
        virtualCamera.m_Follow = playerTransform;
        virtualCamera.m_Priority = 10;
        
        // Configuración del lens
        virtualCamera.m_Lens.OrthographicSize = orthographicSize;
        virtualCamera.m_Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
        
        // Posicionar la cámara a la distancia correcta
        vcamObj.transform.position = new Vector3(
            playerTransform.position.x + offsetX, 
            playerTransform.position.y + offsetY, 
            cameraDistance
        );
        
        // Configurar el componente transposer (clave para movimiento suave)
        CinemachineFramingTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (transposer == null)
        {
            transposer = virtualCamera.AddCinemachineComponent<CinemachineFramingTransposer>();
        }
        
        // Configurar damping (suavizado del movimiento)
        transposer.m_XDamping = xDamping;
        transposer.m_YDamping = yDamping;
        transposer.m_ZDamping = 0f; // No necesario en 2D
        
        // Configurar dead zone (evita que la cámara se mueva con pequeños movimientos)
        transposer.m_DeadZoneWidth = deadZoneWidth;
        transposer.m_DeadZoneHeight = deadZoneHeight;
        
        // Configurar los límites de movimiento para suavizado al llegar a los bordes
        transposer.m_SoftZoneWidth = 0.8f; 
        transposer.m_SoftZoneHeight = 0.8f;
        
        // Configurar el offset
        transposer.m_TrackedObjectOffset = new Vector3(offsetX, offsetY, 0);
        
        // Configurar el confinador
        if (boundingShape != null)
        {
            CinemachineConfiner confiner = virtualCamera.GetComponent<CinemachineConfiner>();
            if (confiner == null)
            {
                confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner>();
            }
            
            confiner.m_BoundingShape2D = boundingShape;
            confiner.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;
            confiner.m_Damping = 0.2f; // Suaviza el movimiento cuando llega a los bordes
            confiner.InvalidatePathCache();
        }
    }
    
    public void UpdateDampingValues(float newXDamping, float newYDamping)
    {
        if (virtualCamera == null) return;
        
        CinemachineFramingTransposer transposer = 
            virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            
        if (transposer != null)
        {
            transposer.m_XDamping = newXDamping;
            transposer.m_YDamping = newYDamping;
        }
    }
    
    public void UpdateDeadZone(float width, float height)
    {
        if (virtualCamera == null) return;
        
        CinemachineFramingTransposer transposer = 
            virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            
        if (transposer != null)
        {
            transposer.m_DeadZoneWidth = width;
            transposer.m_DeadZoneHeight = height;
        }
    }
}