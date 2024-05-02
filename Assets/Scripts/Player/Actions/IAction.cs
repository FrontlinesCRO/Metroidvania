using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Player.Actions
{
    public interface IAction
    {
        PlayerController Player { get; }
        
        void Initialize(PlayerController player);
        void Dispose();
        void Reset();
        void Perform();
        void Cancel();
        void Update(float dt);
    }
}
