using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;

namespace EmotionsFromMotion
{
    public class fLayer:IDisposable
    {
        readonly string myKey = "";
        FaceServiceClient fClient;
        public fLayer()
        {
            fClient = new FaceServiceClient(myKey);
        }
        public async Task<PersonGroup[]> GetMyPersonGroups()
        {
            return await fClient.ListPersonGroupsAsync();
        }
        public async Task<Person[]> GetMyPersons(string groupID)
        {
            return await fClient.GetPersonsAsync(groupID);
        }
        public async Task AddPersonGroup(string groupName)
        {
            await fClient.CreatePersonGroupAsync(("GID-"+groupName).ToLower(),groupName );
        }
        public async Task<CreatePersonResult> AddPerson(string groupID, string personName)
        {
            return await fClient.CreatePersonAsync(groupID, personName);
        }
        public async Task<Face[]> DetectFace(Stream imageStream)
        {
            await Task.Delay(3000);
            return await fClient.DetectAsync(imageStream);
        }
        public async Task<AddPersistedFaceResult> AddFaseToPerson(string personGroupID,Guid personID,Stream imageStream)
        {
            return await fClient.AddPersonFaceAsync(personGroupID,personID,imageStream);
        }
        public async Task<Guid[]> GetPersonFaceIDs(string personGroupID,Guid personID)
        {
            var s = await fClient.GetPersonAsync(personGroupID, personID);
            return s.PersistedFaceIds;
            
        }
        public void Dispose()
        {
            ((IDisposable)fClient).Dispose();
        }
        public async Task TrainGroup(string personGroupID)
        {
            await fClient.TrainPersonGroupAsync(personGroupID);
        }
        public async Task<IdentifyResult[]> Identify(string personGroupID, Guid[] faceIDs)
        {
            await Task.Delay(3000);
            return await fClient.IdentifyAsync(personGroupID, faceIDs, 1);
        }
    }
}
