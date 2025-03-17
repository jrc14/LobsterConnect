using CommunityToolkit.Maui.Views;
using LobsterConnect.VM;
namespace LobsterConnect.V;

public partial class PopupPersonDetails : Popup
{
	public PopupPersonDetails()
	{
        InitializeComponent();

        V.Utilities.StylePopupButtons(this.btnOk, this.btnCancel, this.rdefButtons);
    }

    public void SetPerson(Person person)
    {
        this._person = person;

        this.lbPersonDetails.Text = "Person Details for " + person.Handle;

        this.entryFullName.Text = this.initialFullName = person.FullName;
        this.entryPhoneNumber.Text = this.initialPhoneNumber = person.PhoneNumber;
        this.entryEmail.Text = this.initialEmail = person.Email;

    }

    async void OnOkClicked(object sender, EventArgs e)
    {
        string newFullName = null;
        string newPhoneNumber = null;
        string newEmail = null;

        if(this.entryFullName.Text!= this.initialFullName)
        {
            if(this._person.FullName!= this.initialFullName)
            {
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.WARNING, "Update to full name has been lost, because someone else updated the database while you were working on it");
            }
            else
            {
                newFullName = this.entryFullName.Text;
            }    
        }

        if (this.entryPhoneNumber.Text != this.initialPhoneNumber)
        {
            if (this._person.PhoneNumber != this.initialPhoneNumber)
            {
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.WARNING, "Update to phone number has been lost, because someone else updated the database while you were working on it");
            }
            else
            {
                newPhoneNumber = this.entryPhoneNumber.Text;
            }
        }

        if (this.entryEmail.Text != this.initialEmail)
        {
            if (this._person.Email != this.initialEmail)
            {
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.WARNING, "Update to email has been lost, because someone else updated the database while you were working on it");
            }
            else
            {
                newEmail = this.entryEmail.Text;
            }
        }

        if (newFullName != null || newPhoneNumber != null || newEmail != null)
        {
            try
            {
                MainViewModel.Instance.UpdatePerson(true, _person, newFullName, newPhoneNumber, newEmail);
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.INFO, "Details for '" + _person.Handle + "' have been updated");
            }
            catch(Exception ex)
            {
                MainViewModel.Instance.LogUserMessage(Model.Logger.Level.ERROR, "Error saving details for '" + _person.Handle + "': "+ex.Message);
            }
        }

        await CloseAsync(true, CancellationToken.None);
    }

    async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(false, CancellationToken.None);
    }

    private Person _person=null;

    private string initialFullName;
    private string initialPhoneNumber;
    private string initialEmail;
}