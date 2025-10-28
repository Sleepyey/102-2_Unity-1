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
    private Vector3 camOffset = new Vector3(0f, 0.4f, 0f);     // ī�޶� ��ġ

    [Header("Dash")]
    private bool isDash;
    private float dashSpeed;
    private float dashDuration = 0.2f;
    Vector3 dashDir;
    bool isDashing;
    float dashTimer;

    [Header("Weapon")]
    public Transform weaponT;     // �� �� ��ġ
    public Weapon currentWeapon;  // ���� ��� �ִ� ����

    [Header("Interact")]
    [SerializeField] float interactDistance = 3f;
    [SerializeField] LayerMask interactMask = ~0;   // ��� ���̾�
    [SerializeField] KeyCode interactKey = KeyCode.F;
    [SerializeField] Text promptText;

    [Header("HPBar")]
    public Slider hpBar;

    private float airC = 0.2f; // 0f ~ 1f = ���߿����� �̵� ����
    Vector3 airDir;

    private float speedV;                       // �������� �̵� �ӵ�
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

        if (!cam) cam = Camera.main;                // ī�޶� ������ ī�޶� ����
        cam.transform.SetParent(transform);         // ī�޶� �÷��̾��� �ڽ����� ����
        cam.transform.localPosition = camOffset;    // ī�޶� ��ġ ����
        cam.nearClipPlane = 0.1f;                   // ��ü ������ �Ÿ�

        Cursor.lockState = CursorLockMode.Locked;           // ���콺 Ŀ�� �߾� ����
        Cursor.visible = false;                             // ���콺 Ŀ�� ����

        // GameManager���� ü�� ������ �޾ƿ���
        if (GameManager.instance != null)
        {
            // currentHP�� �� ä���� ������ Ǯ�Ƿ� ����
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

        if (hpBar) hpBar.value = (float)currentHP / (float)maxHP;   // �׽�Ʈ�� ���� if �߰�
        //- ���� �ð��� �ȴٸ� ü�¹� �ؿ� ���ڷ� �ִ� ü�� �� ���� ü�� �߰� ���� -
    }

    // Update
    void Update()
    {
        // ���콺 ȭ��
        float mx = Input.GetAxis("Mouse X") * mouseSpeed;
        float my = Input.GetAxis("Mouse Y") * mouseSpeed;

        transform.Rotate(0f, mx, 0f);   // ĳ���� ��ü�� �ٶ󺸴� �������� ȸ��
        mouseY = Mathf.Clamp(mouseY - my, mouseYMin, mouseYMax);    // ���� ���� Clamp�� ���� ����
        if (cam) cam.transform.localRotation = Quaternion.Euler(mouseY, 0f, 0f);    // ���� ȭ�� ȸ��


        // ���� ���� �������� Ȯ��
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;     // ���鿡 ���̱� + �߷� �ʱ�ȭ


        // WASD
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.right * x +  transform.forward * z).normalized;   // �̵� ����


        // �޸���
        speedV = Input.GetKey(KeyCode.LeftShift) && isGrounded ? moveSpeed * sprint : moveSpeed;


        // Jump
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpPower;
            airDir = move * speedV;                         // �ϴÿ����� �ӵ�
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


        // �߷�
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
            Vector3 airMove = move * moveSpeed;     // ���� �⺻ �ӵ�
            if (move.sqrMagnitude > 0.0001f) airMove = move.normalized * airDir.magnitude;
            else airMove = airDir;

            airDir = Vector3.Lerp(airDir, airMove, airC * Time.deltaTime * 10f);
            moveH = airDir;
        }

        Vector3 moveA = moveH + Vector3.up * velocity.y;
        controller.Move(moveA * Time.deltaTime);

        // ��� & ���� �Է�
        WeaponInput();


        // ��ȣ�ۿ�
        Interact();
    }

    void WeaponInput()
    {
        if (currentWeapon == null) return;

        bool toggle = Input.GetMouseButtonDown(0);  // �ܹ�
        bool hold = Input.GetMouseButton(0);        // ����
        bool reload = Input.GetKeyDown(KeyCode.R);  // ����

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

        // ��ȣ�ۿ� (Key)
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

        // �� ���� ����
        Weapon newWeapon = Instantiate(weaponPrefab, weaponT);
        newWeapon.transform.localPosition = Vector3.zero;

        newWeapon.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);  // �ΰ��� ���� �ذ��

        // ���Ⱑ ������ ī�޶� ����
        newWeapon.cam = cam;

        currentWeapon = newWeapon;
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);   // ü���� �ִ� ü�� �̻����� ȸ�� x

        if (hpBar) hpBar.value = (float)currentHP / (float)maxHP;

        if (GameManager.instance != null) GameManager.instance.playerCurrentHP = currentHP;
    }

    //- �޽����� �Բ� ���� ȭ������ �̵� ���� -
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
