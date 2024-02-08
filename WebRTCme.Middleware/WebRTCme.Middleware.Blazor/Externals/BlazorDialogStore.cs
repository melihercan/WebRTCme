using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDialog
{
    internal class BlazorDialogStore2 : IBlazorDialogStore
    {
        private Dictionary<string, Dialog> registeredDialogs = new Dictionary<string, Dialog>();

        public event Func<Task> OnComponentAsDialogsChanged;

        public Dialog GetById(string id)
        {
            if (registeredDialogs.ContainsKey(id))
            {
                return registeredDialogs[id];
            }

            throw new ArgumentException($"No dialog found for id '{id}'", nameof(id));
        }

        public Dictionary<string, ComponentDialog> GetComponentsAsDialogs()
        {
            throw new NotImplementedException();
        }

        public int GetVisibleDialogsCount()
        {
            return registeredDialogs.Count(x => x.Value.GetVisibility());
        }

        public void Register(Dialog blazorDialog)
        {
            if (blazorDialog?.Id == null)
            {
                throw new ArgumentException("BlazorDialog Id is null", nameof(blazorDialog));
            }
            registeredDialogs[blazorDialog.Id] = blazorDialog;
        }

        public Task RegisterComponentDialog(string id, ComponentDialog compDialog)
        {
            throw new NotImplementedException();
        }

        public void Unregister(Dialog blazorDialog)
        {
            if (blazorDialog.Id != null && registeredDialogs.ContainsKey(blazorDialog.Id))
            {
                registeredDialogs.Remove(blazorDialog.Id);
            }
        }

        public Task UnregisterComponentDialog(string id)
        {
            throw new NotImplementedException();
        }
    }
}
