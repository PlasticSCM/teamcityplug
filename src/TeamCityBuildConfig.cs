using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TeamCityPlug
{
    /* https://confluence.jetbrains.com/display/TCD10/REST+API#RESTAPI-TriggeringaBuild
      <build personal="true" branchName="logicBuildBranch">
        <buildType id="buildConfID"/>
        <comment>
          <text>build triggering comment</text>
        </comment>
        <properties>
          <property name="env.myEnv" value="bbb"/>
        </properties>
      </build>
     */
    [XmlRoot(ElementName = "build")]
    public class TeamCityBuildConfig
    {
        [XmlAttribute]
        public bool personal = false;

        public BuildType buildType = new BuildType();

        public BuildComment comment = new BuildComment();

        [XmlArrayItem(ElementName = "property", Type = typeof(BuildProperty))]
        [XmlArray("properties")]
        public BuildProperty[] properties = new BuildProperty[0];

        public string SerializeToXml()
        {
            var emptyNamepsaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(this.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, this, emptyNamepsaces);
                return stream.ToString();
            }
        }
    }

    [XmlRoot(ElementName = "buildType")]
    public class BuildType
    {
        [XmlAttribute]
        public string id = string.Empty;
    }

    [XmlRoot(ElementName = "comment")]
    public class BuildComment
    {
        public string text = string.Empty;
    }

    [XmlRoot(ElementName = "property")]
    public class BuildProperty
    {
        [XmlAttribute]
        public string name = string.Empty;
        [XmlAttribute]
        public string value = string.Empty;
    }
}
