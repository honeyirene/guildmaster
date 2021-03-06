using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GuildMaster.Characters;
using GuildMaster.Data;
using GuildMaster.Databases;
using GuildMaster.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace GuildMaster.Windows
{
    public class ExplorationCharacterSelectingWindow : DraggableWindow
    {
        [SerializeField] private Transform characterSelectingListParent;
        [SerializeField] private Toggle characterSelectingTogglePrefab;
        [SerializeField] private ToggleGroup characterToggleGroup;
        [SerializeField] private Transform characterSelectedListParent;
        [SerializeField] private Toggle characterSelectedTogglePrefab;
        [SerializeField] private Image characterIllustration;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text loyaltyLabel;
        [SerializeField] private Text HpLabel;
        [SerializeField] private Text StaminaLabel;
        [SerializeField] private Text StrengthLabel;
        [SerializeField] private Text TrickLabel;
        [SerializeField] private Text WisdomLabel;
        [SerializeField] private Text CharacteristicLabel;

        public class Response
        {
            public enum ActionEnum
            {
                Cancel,
                GoNext
            }

            public ActionEnum NextAction;
            public List<Character> SelectedCharacters;
        }

        public void OpenNext()
        {
            _responseTaskCompletionSource.TrySetResult(
                new Response
                {
                    NextAction = Response.ActionEnum.GoNext,
                    SelectedCharacters = _exploreCharacterList.ToList(),
                });
        }

        protected override void OnClose()
        {
            _responseTaskCompletionSource.TrySetResult(
                new Response
                {
                    NextAction = Response.ActionEnum.Cancel,
                    SelectedCharacters = null,
                });
        }

        public async Task<Response> GetResponse(List<Character> initialSelectedCharacters = null,
            CancellationToken cancellationToken = default)
        {
            if (initialSelectedCharacters == null)
                initialSelectedCharacters = new List<Character>();
            
            return await _getResponseSingularRun.Run(async linkedCancellationToken =>
            {
                try
                {
                    // 윈도우 초기 화면
                    base.OpenWindow();
                    ResetCharacterLists(initialSelectedCharacters);
                    RefreshList();

                    // 입력 기다림.
                    _responseTaskCompletionSource = new TaskCompletionSource<Response>();
                    return await _responseTaskCompletionSource.CancellableTask(linkedCancellationToken);
                }
                finally
                {
                    Close();
                }
            }, cancellationToken);
        }

        public void SwitchList()
        {
            if (_allCharacters.Contains(_currentCharacter))
            {
                if (_exploreCharacterList.Count == 4) return;
                _allCharacters.Remove(_currentCharacter);
                _exploreCharacterList.Add(_currentCharacter);
                RefreshList();
            }
            else if (_exploreCharacterList.Contains(_currentCharacter))
            {
                _exploreCharacterList.Remove(_currentCharacter);
                _allCharacters.Add(_currentCharacter);
                RefreshList();
            }
        }

        private void RefreshList()
        {
            SetCharacter(null);
            foreach (Transform t in characterSelectingListParent)
                Destroy(t.gameObject);
            foreach (var (ch, i) in _allCharacters.Select((i, j) =>
                (i, j)))
            {
                var made = Instantiate(characterSelectingTogglePrefab, characterSelectingListParent);
                made.group = characterToggleGroup;
                made.GetComponentInChildren<Text>().text = ch.UsingName;
                var capture = ch;
                made.onValueChanged.AddListener(b =>
                {
                    if (b) SetCharacter(capture);
                });
                if (i == 0)
                    made.isOn = false; //위의 AddListener와 순서 주의.
            }

            foreach (Transform t in characterSelectedListParent)
                Destroy(t.gameObject);
            foreach (var (ch, i) in _exploreCharacterList.Select((i, j) =>
                (i, j)))
            {
                var made = Instantiate(characterSelectedTogglePrefab, characterSelectedListParent);
                made.group = characterToggleGroup;
                made.GetComponentInChildren<Text>().text = ch.UsingName;
                var capture = ch;
                made.onValueChanged.AddListener(b =>
                {
                    if (b) SetCharacter(capture);
                });
                if (i == 0)
                    made.isOn = false; //위의 AddListener와 순서 주의.
            }
        }

        private void SetCharacter(Character character)
        {
            Unsubscribe();
            _currentCharacter = character;
            if (_currentCharacter != null) 
                _currentCharacter.Changed += Refresh;
            Refresh();
        }

        private void Unsubscribe()
        {
            if (_currentCharacter != null)
                _currentCharacter.Changed -= Refresh;
        }

        private void OnDestroy() => Unsubscribe();

        private void Refresh()
        {
            if (_currentCharacter == null)
            {
                // Todo: 뭔가 선택된 캐릭터가 없는 경우의 화면 보여주기.
                nameLabel.text = "선택된 캐릭터 없음.";
                return;
            }

            var sd = _currentCharacter.StaticData;
            characterIllustration.sprite = sd.BasicData.Illustration;
            nameLabel.text = _currentCharacter.UsingName;
            loyaltyLabel.text = _currentCharacter.Loyalty.ToString();

            string TraitText(Character character)
            {
                return string.Join("\n", character.ActiveTraits
                    .Select(TraitDatabase.Get)
                    .Select(tsd => $"[{tsd.Name}]\n{tsd.Description}"));
            }

            CharacteristicLabel.text = TraitText(_currentCharacter);
            HpLabel.text = $"{_currentCharacter.Hp}/{_currentCharacter.MaxHp}";
            StaminaLabel.text = $"{_currentCharacter.Stamina}/{_currentCharacter.MaxStamina}";
            StrengthLabel.text = _currentCharacter.Strength.ToString();
            TrickLabel.text = _currentCharacter.Trick.ToString();
            WisdomLabel.text = _currentCharacter.Wisdom.ToString();
            //유니티 상에서의 수정 필요
        }

        private void ResetCharacterLists(List<Character> initialExploreCharacterList)
        {
            _allCharacters = Player.Instance.PlayerGuild._guildMembers.GuildMemberList.ToList();
            _exploreCharacterList.Clear();
            foreach (var c in initialExploreCharacterList)
            {
                if (_allCharacters.Contains(c))
                {
                    _allCharacters.Remove(c);
                    _exploreCharacterList.Add(c);
                }
                else
                {
                    Debug.LogWarning($"{nameof(_allCharacters)} doesn't contains {c} from {nameof(initialExploreCharacterList)}");
                }
            }
        }


        private readonly List<Character> _exploreCharacterList = new List<Character>();
        private readonly SingularRun _getResponseSingularRun = new SingularRun();
        private Character _currentCharacter;
        private List<Character> _allCharacters;
        private TaskCompletionSource<Response> _responseTaskCompletionSource = new TaskCompletionSource<Response>();
    }
}