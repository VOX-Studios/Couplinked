using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.Multiplayer
{
    class PlayerSlotsManager
    {
        private int _maxNumberOfPlayers;
        private List<int> _playerSlots;
        public PlayerSlotsManager(int maxNumberOfPlayers)
        {
            _maxNumberOfPlayers = maxNumberOfPlayers;

            _playerSlots = new List<int>();

            for(int i = 0; i < _maxNumberOfPlayers; i++)
            {
                _playerSlots.Add(i);
            }
        }

        /// <summary>
        /// Reservers the next available slot if one is available.  Returns -1 if no slots are available;
        /// </summary>
        /// <returns>Returns -1 if no slots are available</returns>
        public int TakeSlot()
        {
            if(_playerSlots.Count == 0)
            {
                return -1;
            }

            int slot = _playerSlots[0];
            _playerSlots.RemoveAt(0);
            return slot;
        }

        public bool ReturnSlot(int i)
        {
            if(i < 0 || i >= _maxNumberOfPlayers)
            {
                return false;
            }

            _playerSlots.Add(i);
            _playerSlots.Sort();
            return true;
        }
    }
}
