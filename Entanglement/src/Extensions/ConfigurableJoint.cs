using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Entanglement.Extensions {
    public static class ConfigurableJointExtensions {
        public static void SetMotion(this ConfigurableJoint joint, ConfigurableJointMotion motion) {
            joint.xMotion = joint.yMotion = joint.zMotion = joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = motion;
        }

        public static void SetDrive(this ConfigurableJoint joint, float spring, float damper, float maximumForce = float.MaxValue) {
            JointDrive drive = new JointDrive();
            drive.positionSpring = spring;
            drive.positionDamper = damper;
            drive.maximumForce = maximumForce;
            joint.rotationDriveMode = RotationDriveMode.Slerp;
            joint.xDrive = joint.yDrive = joint.zDrive = joint.slerpDrive = drive;
        }
    }
}
