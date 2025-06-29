using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public enum TurnPhase
{
    WaitingForMoveSelection,
    WaitingForFingerSelection,
    WaitingForShout,
    ShowingYubisuma,
    ApplyingMove,
    SwitchingTurn
}

public class GameManager : MonoBehaviour
{
    [Header("Back Button")]
    public Button backButton;

    public List<Player> players = new List<Player>();
    public int currentPlayerIndex = 0;

    public FingerInputUI fingerInputUI;
    public TurnManagerUI turnManagerUI;
    public StatusDisplayUI statusDisplayUI;
    public GameOverUI gameOverUI;
    public MoveSelectionUI moveSelectionUI;

    public TextMeshProUGUI yubisumaText;
    public Button shoutButton;
    public TextMeshProUGUI moveLogText;
    public Button returnToTitleButton;
    public GunTargetUI gunTargetUI;

    private bool gameEnded = false;
    private MoveType? selectedMoveType = null;
    private TurnPhase turnPhase = TurnPhase.WaitingForMoveSelection;
    private List<string> moveLogHistory = new List<string>();
    private Player currentGunTarget;
    private bool isGunTargetingMode = false;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (GameSettings.playerMoves == null || GameSettings.playerMoves.Count != 10)
        {
            Debug.Log("playerMoves が空なので defaultDeck を使用");
            GameSettings.playerMoves = new List<MoveType>(GameSettings.defaultDeck);
        }
        players = new List<Player>();

        var player = new Player("You");
        player.MoveSet = GameSettings.playerMoves;
        players.Add(player);

        for (int i = 1; i <= GameSettings.cpuCount; i++)
        {
            var cpu = new Player($"CPU{i}", true);
            cpu.MoveSet = GenerateRandomMoveset();
            players.Add(cpu);
        }

        if (!GameSettings.useCpuOpponent && GameSettings.cpuCount == 0)
            players.Add(new Player("Player 2"));

        moveSelectionUI.Init(this);
        fingerInputUI.SetCurrentPlayer(GetCurrentPlayer());
        turnManagerUI.UpdateTurnText(GetCurrentPlayer().Name);

        if (players.Count >= 2)
            statusDisplayUI.UpdateStatus(players);

        HideYubisumaText();
        shoutButton.gameObject.SetActive(false);
        moveLogText.text = string.Empty;
        returnToTitleButton.gameObject.SetActive(false);

        // 戻るボタンは最初は非表示＆クリックリスナを登録
        backButton.gameObject.SetActive(false);
        backButton.onClick.AddListener(OnBackToMoveSelection);

        if (GetCurrentPlayer().isCPU)
            StartCoroutine(StartCpuTurn());
        else
            StartPlayerTurn();

        fingerInputUI.Show();
    }

    public Player GetCurrentPlayer() => players[currentPlayerIndex];

    public void AppendMoveLog(string message)
    {
        moveLogHistory.Clear();
        moveLogHistory.Add(message);
        moveLogText.text = message;
    }

    void StartPlayerTurn()
    {
        if (!GetCurrentPlayer().CanAct())
        {
            AppendMoveLog($"{GetCurrentPlayer().Name} is paralyzed and cannot move!");
            StartCoroutine(SwitchTurnWithDelay());
            return;
        }

        fingerInputUI.SetCurrentPlayer(GetCurrentPlayer());
        if (isGunTargetingMode) return;

        selectedMoveType = null;
        turnPhase = TurnPhase.WaitingForMoveSelection;
        moveSelectionUI.Init(this);
        moveSelectionUI.Show();
    }

    public void OnMoveSelected(MoveType moveType)
    {
        selectedMoveType = moveType;
        moveSelectionUI.Hide();
        // 戻るボタンを表示
        backButton.gameObject.SetActive(true);

        shoutButton.onClick.RemoveAllListeners();
        if (moveType == MoveType.Gun)
        {
            BeginGunTargeting(GetCurrentPlayer());
            return;
        }

        shoutButton.onClick.AddListener(OnShoutButtonPressed);
        turnPhase = TurnPhase.WaitingForFingerSelection;
        shoutButton.gameObject.SetActive(true);
    }

    public void OnShoutButtonPressed()
    {
        shoutButton.gameObject.SetActive(false);
        // 戻るボタンを非表示
        backButton.gameObject.SetActive(false);
        StartCoroutine(ShowYubisumaThenApply());
    }

    public void OnGunShoutButtonPressed()
    {
        shoutButton.gameObject.SetActive(false);
        StartCoroutine(HandleGunSmashSequence());
    }

    IEnumerator HandleGunSmashSequence()
    {
        if (currentGunTarget == null) yield break;

        Player attacker = GetCurrentPlayer();
        Player target = currentGunTarget;
        attacker.FingersUp[0] = attacker.FingersUp[1] = false;
        target.FingersUp[0] = target.FingersUp[1] = false;

        ShowYubisumaText("SMASH!!");
        fingerInputUI.SetCurrentPlayer(attacker);
        fingerInputUI.SetGunMode(true);
        fingerInputUI.Show();

        if (target.isCPU) target.RandomizeSingleFinger();
        yield return new WaitForSeconds(2f);
        HideYubisumaText();

        if (!target.FingersUp[0] && !target.FingersUp[1]) target.RandomizeSingleFinger();
        int guessedFinger = fingerInputUI.GetSelectedFinger();
        fingerInputUI.SetGunMode(false);

        ResolveGunAttack(attacker, target, guessedFinger);
    }

    IEnumerator ShowYubisumaThenApply()
    {
        turnPhase = TurnPhase.ShowingYubisuma;
        Player self = GetCurrentPlayer();
        foreach (var p in players) p.FingersUp[0] = p.FingersUp[1] = false;
        foreach (var p in players) if (p != self && p.isCPU) p.RandomizeFingers();

        ShowYubisumaText("SMASH!!");
        fingerInputUI.Show();
        yield return new WaitForSeconds(2f);
        HideYubisumaText();

        turnPhase = TurnPhase.ApplyingMove;
        List<Player> targets = players.Where(p => p != self && p.FingersUp.Any(up => up)).ToList();

        Move move = new Move(selectedMoveType.Value);
        if (move.CanActivate(self, targets))
        {
            move.ApplyEffect(self, targets, players);
            LogDamage(self, targets, move.Name, 1);
        }
        else
        {
            AppendMoveLog($"{self.Name}'s move failed");
        }

        statusDisplayUI.UpdateStatus(players);
        CheckGameOver();

        if (!gameEnded)
        {
            yield return new WaitForSeconds(1.5f);
            foreach (var p in players) p.FingersUp[0] = p.FingersUp[1] = false;
            fingerInputUI.SetCurrentPlayer(self);
            fingerInputUI.UpdateFingerDisplay();
            statusDisplayUI.UpdateStatus(players);
            StartCoroutine(SwitchTurnWithDelay());
        }
    }

    IEnumerator StartCpuTurn()
    {
        Player cpu = GetCurrentPlayer();
        if (!cpu.CanAct())
        {
            AppendMoveLog($"{cpu.Name} is paralyzed and cannot move!");
            yield return new WaitForSeconds(1f);
            StartCoroutine(SwitchTurnWithDelay());
            yield break;
        }

        Move move = cpu.ChooseMove(players);
        if (move.Type == MoveType.Gun)
        {
            BeginGunTargeting(cpu);
            yield break;
        }

        turnPhase = TurnPhase.ShowingYubisuma;
        foreach (var p in players) p.FingersUp[0] = p.FingersUp[1] = false;

        ShowYubisumaText("SMASH!!");
        cpu.RandomizeFingers();

        Player humanTarget = players.First(p => !p.isCPU && p.IsAlive);
        fingerInputUI.SetCurrentPlayer(humanTarget);
        fingerInputUI.SetGunMode(true);
        fingerInputUI.Show();

        float timer = 0f; const float TIMEOUT = 1.5f;
        while (timer < TIMEOUT && !humanTarget.HasSelectedFinger())
        {
            timer += Time.deltaTime;
            yield return null;
        }
        HideYubisumaText();

        if (!humanTarget.HasSelectedFinger()) humanTarget.FingersUp[0] = humanTarget.FingersUp[1] = false;
        turnPhase = TurnPhase.ApplyingMove;
        List<Player> targets2 = players.Where(p => p != cpu && p.FingersUp.Any(up => up)).ToList();
        if (move.CanActivate(cpu, targets2))
        {
            move.ApplyEffect(cpu, targets2, players);
            LogDamage(cpu, targets2, move.Name, 1);
        }
        else
        {
            AppendMoveLog($"{cpu.Name}'s move failed");
        }

        statusDisplayUI.UpdateStatus(players);
        CheckGameOver();

        if (!gameEnded)
        {
            yield return new WaitForSeconds(1.5f);
            foreach (var p in players) p.FingersUp[0] = p.FingersUp[1] = false;
            fingerInputUI.SetCurrentPlayer(players[(currentPlayerIndex + 1) % players.Count]);
            fingerInputUI.UpdateFingerDisplay();
            statusDisplayUI.UpdateStatus(players);
            StartCoroutine(SwitchTurnWithDelay());
        }
    }

    IEnumerator SwitchTurnWithDelay()
    {
        turnPhase = TurnPhase.SwitchingTurn;
        moveSelectionUI.Hide();
        yield return new WaitForSeconds(1f);

        selectedMoveType = null;
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        while (!players[currentPlayerIndex].CanAct())
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        turnManagerUI.UpdateTurnText(GetCurrentPlayer().Name);
        if (GetCurrentPlayer().isCPU)
            StartCoroutine(StartCpuTurn());
        else
            StartPlayerTurn();
    }

    void CheckGameOver()
    {
        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (players[i].JustDied)
            {
                AppendMoveLog($"{players[i].Name} has been defeated!");
                players.RemoveAt(i);
            }
        }
        if (players.Count == 1)
        {
            string winnerName = players[0].Name;
            gameOverUI.Show(winnerName);
            gameEnded = true;
            fingerInputUI.UpdateFingerDisplay();
            statusDisplayUI.UpdateStatus(players);
            returnToTitleButton.gameObject.SetActive(true);
        }
    }

    void ShowYubisumaText(string message)
    {
        yubisumaText.text = message;
        yubisumaText.gameObject.SetActive(true);
    }

    void HideYubisumaText()
    {
        yubisumaText.text = string.Empty;
        yubisumaText.gameObject.SetActive(false);
    }

    public void OnReturnToTitleButtonClicked()
    {
        SceneManager.LoadScene("TitleScene");
    }

    /// <summary>
    /// 技選択画面に戻る
    /// </summary>
    public void OnBackToMoveSelection()
    {
        shoutButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        selectedMoveType = null;
        turnPhase = TurnPhase.WaitingForMoveSelection;

        moveSelectionUI.Init(this);
        moveSelectionUI.Show();
    }

    public void LogDamage(Player attacker, List<Player> targets, string moveName, int damage)
    {
        if (targets.Count == 0)
        {
            AppendMoveLog($"{attacker.Name} used {moveName}, but hit no one.");
            return;
        }
        string log = $"{attacker.Name} used {moveName} → ";
        log += string.Join(", ", targets.Select(t => $"{t.Name} -{damage}"));
        AppendMoveLog(log);
    }

    public void BeginGunTargeting(Player self)
    {
        var candidates = players.Where(p => p != self && p.IsAlive).ToList();
        if (candidates.Count == 1)
        {
            currentGunTarget = candidates[0];
            shoutButton.onClick.RemoveAllListeners();
            shoutButton.onClick.AddListener(OnGunShoutButtonPressed);
            shoutButton.gameObject.SetActive(true);
            return;
        }
        gunTargetUI.Show(self, candidates, (target) =>
        {
            currentGunTarget = target;
            shoutButton.onClick.RemoveAllListeners();
            shoutButton.onClick.AddListener(OnGunShoutButtonPressed);
            shoutButton.gameObject.SetActive(true);
        });
        shoutButton.gameObject.SetActive(false);
    }

    public void ResolveGunAttack(Player attacker, Player target, int guessedFingerIndex)
    {
        bool isHit = guessedFingerIndex >= 0 && guessedFingerIndex < target.FingersUp.Length
            ? target.FingersUp[guessedFingerIndex]
            : true;

        if (isHit)
        {
            target.TakeDamage(4, attacker.HasPiercing);
            AppendMoveLog($"{attacker.Name}の銃が命中!{target.Name}に4ダメージ!");
        }
        else
        {
            AppendMoveLog($"{attacker.Name}の銃は外れた!");
        }

        attacker.HasPiercing = false;
        currentGunTarget = null;

        statusDisplayUI.UpdateStatus(players);
        CheckGameOver();
        if (gameEnded) return;

        StartCoroutine(SwitchTurnWithDelay());
    }

    public void EndTurn()
    {
        StartCoroutine(SwitchTurnWithDelay());
    }

    private List<MoveType> GenerateRandomMoveset()
    {
        List<MoveType> all = new List<MoveType>(
            System.Enum.GetValues(typeof(MoveType)) as MoveType[]
        );
        all.Remove(MoveType.Heal);
        all.Remove(MoveType.Cure);
        all = all.OrderBy(x => Random.value).Take(8).ToList();
        all.Insert(0, MoveType.Heal);
        all.Insert(1, MoveType.Cure);
        return all;
    }
}
