using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private static PlayerController instance;

    [Header("Stat")]
    public int maxHP = 100;
    public int currentHP;
    private float moveSpeed = 4f;
    private float sprint = 2f;
    private float jumpPower = 4f;

    [Header("Mouse")]
    public float mouseSpeed = 2f;
    private float mouseYMin = -90f;
    private float mouseYMax = 90f;
    private float mouseY;

    [Header("Camera")]
    public Camera cam;
    private Vector3 camOffset = new Vector3(0f, 0.4f, 0f);     // 카메라 위치

    [Header("Dash")]
    private bool isDash;
    private float dashSpeed;
    private float dashDuration = 0.2f;
    Vector3 dashDir;
    bool isDashing;
    float dashTimer;

    [Header("Weapon")]
    public Transform weaponT;     // 총 들 위치
    public Weapon currentWeapon;  // 현재 들고 있는 무기

    [Header("Interact")]
    [SerializeField] float interactDistance = 3f;
    [SerializeField] LayerMask interactMask = ~0;   // 모든 레이어
    [SerializeField] KeyCode interactKey = KeyCode.F;
    [SerializeField] Text promptText;

    [Header("HPBar")]
    public Slider hpBar;

    private float airC = 0.2f; // 0f ~ 1f = 공중에서의 이동 저항
    Vector3 airDir;

    private float speedV;                       // 실질적인 이동 속도
    private float gravity = -9.81f;
    private bool isGrounded;
    private Vector3 velocity;

    private CharacterController controller;

    IInteractable currentFocus;

    // Awake
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    // Start
    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (!cam) cam = Camera.main;                // 카메라가 없으면 카메라 지정
        cam.transform.SetParent(transform);         // 카메라를 플레이어의 자식으로 지정
        cam.transform.localPosition = camOffset;    // 카메라 위치 조정
        cam.nearClipPlane = 0.1f;                   // 물체 렌더링 거리

        Cursor.lockState = CursorLockMode.Locked;           // 마우스 커서 중앙 고정
        Cursor.visible = false;                             // 마우스 커서 숨김

        // GameManager에서 체력 데이터 받아오기
        if (GameManager.instance != null)
        {
            // currentHP가 안 채워져 있으면 풀피로 시작
            if (GameManager.instance.playerCurrentHP <= 0)
            {
                GameManager.instance.playerCurrentHP = maxHP;
            }

            currentHP = GameManager.instance.playerCurrentHP;
        }
        else
        {
            if (currentHP <= 0) currentHP = maxHP;
        }

        if (hpBar) hpBar.value = (float)currentHP / (float)maxHP;   // 테스트를 위해 if 추가
        //- 만약 시간이 된다면 체력바 밑에 숫자로 최대 체력 및 현재 체력 추가 예정 -
    }

    // Update
    void Update()
    {
        // 마우스 화면
        float mx = Input.GetAxis("Mouse X") * mouseSpeed;
        float my = Input.GetAxis("Mouse Y") * mouseSpeed;

        transform.Rotate(0f, mx, 0f);   // 캐릭터 본체를 바라보는 방향으로 회전
        mouseY = Mathf.Clamp(mouseY - my, mouseYMin, mouseYMax);    // 상하 각도 Clamp로 각도 제한
        if (cam) cam.transform.localRotation = Quaternion.Euler(mouseY, 0f, 0f);    // 상하 화면 회전


        // 땅에 닿은 상태인지 확인
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;     // 지면에 붙이기 + 중력 초기화


        // WASD
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.right * x +  transform.forward * z).normalized;   // 이동 방향


        // 달리기
        speedV = Input.GetKey(KeyCode.LeftShift) && isGrounded ? moveSpeed * sprint : moveSpeed;


        // Jump
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpPower;
            airDir = move * speedV;                         // 하늘에서의 속도
        }


        // Dash
        if (isGrounded && !isDash) isDash = true;           // dash++

        if (!isGrounded && isDash && Input.GetKeyDown(KeyCode.Space))
        {
            if (airDir.sqrMagnitude > 0.0001f)
            {
                isDash = false;
                isDashing = true;
                dashTimer = dashDuration;
                dashDir = airDir.normalized;
                dashSpeed = airDir.magnitude * 2f;       // magnitude = power
            }
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) isDashing = false;
        }


        // 중력
        velocity.y += gravity * Time.deltaTime;


        // Move
        Vector3 moveH;
        if (isDashing)
        {
            moveH = dashDir * dashSpeed;
        }
        else if (isGrounded)
        {
            moveH = move * speedV;
        }
        else
        {
            Vector3 airMove = move * moveSpeed;     // 공중 기본 속도
            if (move.sqrMagnitude > 0.0001f) airMove = move.normalized * airDir.magnitude;
            else airMove = airDir;

            airDir = Vector3.Lerp(airDir, airMove, airC * Time.deltaTime * 10f);
            moveH = airDir;
        }

        Vector3 moveA = moveH + Vector3.up * velocity.y;
        controller.Move(moveA * Time.deltaTime);

        // 사격 & 장전 입력
        WeaponInput();


        // 상호작용
        Interact();
    }

    void WeaponInput()
    {
        if (currentWeapon == null) return;

        bool toggle = Input.GetMouseButtonDown(0);  // 단발
        bool hold = Input.GetMouseButton(0);        // 연사
        bool reload = Input.GetKeyDown(KeyCode.R);  // 장전

        currentWeapon.FireInput(toggle, hold);
        currentWeapon.ReloadInput(reload);
    }

    // TakeDamage
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        if (hpBar) hpBar.value = (float)currentHP / (float)maxHP;

        if (GameManager.instance != null) GameManager.instance.playerCurrentHP = currentHP;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Interact()
    {
        currentFocus = null;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
        {
            currentFocus = hit.collider.GetComponentInParent<IInteractable>();
        }

        // prompt UI
        if (promptText)
        {
            if (currentFocus != null)
            {
                promptText.gameObject.SetActive(true);
                promptText.text = currentFocus.GetPrompt();
            }
            else
            {
                promptText.gameObject.SetActive(false);
            }
        }

        // 상호작용 (Key)
        if (currentFocus != null && Input.GetKeyDown(interactKey))
        {
            currentFocus.Interact(this);    // this = PlayerController
        }
    }

    public void EquipWeapon(Weapon weaponPrefab)
    {
        if (weaponPrefab == null || weaponT == null) return;

        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
            currentWeapon = null;
        }

        // 새 무기 장착
        Weapon newWeapon = Instantiate(weaponPrefab, weaponT);
        newWeapon.transform.localPosition = Vector3.zero;

        newWeapon.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);  // 인게임 오류 해결용

        // 무기가 조준할 카메라 지정
        newWeapon.cam = cam;

        currentWeapon = newWeapon;
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);   // 체력이 최대 체력 이상으로 회복 x

        if (hpBar) hpBar.value = (float)currentHP / (float)maxHP;

        if (GameManager.instance != null) GameManager.instance.playerCurrentHP = currentHP;
    }

    //- 메시지와 함께 메인 화면으로 이동 예정 -
    void Die()
    {
        if (GameManager.instance != null) GameManager.instance.playerCurrentHP = 0;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SceneObject>() != null)
        {
            string info = other.GetComponent<SceneObject>().objectInfo;
            if (info.StartsWith("Scene"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(info);
            }
        }
    }
}
