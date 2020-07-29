using System;
using GuildMaster.Databases;
using GuildMaster.Tools;
using UnityEngine;
using UnityEngine.EventSystems;



namespace GuildMaster.Exploration
{
    using MapNode = Graph<ExplorationMap.NodeContent>.Node;
    
    [RequireComponent(typeof(SpriteRenderer))]
    public class LocationButton: GenericButton<LocationButton>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Sprite _baseLocationSprite;
        [SerializeField] private Sprite _normalLocationSprite;
        protected override LocationButton EventArgument => this;
        public ChangeTrackedValue<bool> IsUnderPointer { get; private set; } = new ChangeTrackedValue<bool>(false);
        
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            IsUnderPointer.Value = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsUnderPointer.Value = false;
        }


        public void SetNode( MapNode node)
        {
            Node = node;
            _here = ExplorationLocationDatabase.Get(Node.Content.LocationCode);
            
            switch (_here.LocationType)
            {
                case Location.Type.Base:
                    _spriteRenderer.sprite = _baseLocationSprite;
                    break;
                case Location.Type.Normal:
                    _spriteRenderer.sprite = _normalLocationSprite;
                    break;
                default:
                    throw new Exception($"Couldn't process the Location type {_here.LocationType}");
            }
        }

        
        public void DefaultState()
        {
            _spriteRenderer.color = Color.white;
        }

        public void Highlight(Color color)
        {
            _spriteRenderer.color = color;
        }
        

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public MapNode Node { get; private set; }

        private SpriteRenderer _spriteRenderer;
        private Location _here;
    }
}