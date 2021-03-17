package ninja.pranav.algorithms.gridpointcode;

import java.util.Objects;

public class Coordinates {
    public final double Latitude;
    public final double Longitude;
    public Coordinates(double latitude, double longitude) {
        this.Latitude = latitude;
        this.Longitude = longitude;
    }
    
    @Override
    public boolean equals(Object obj) {
        if (this == obj) {
            return true;
        }
        if (obj == null || getClass() != obj.getClass()) {
          return false;
        }
    
        Coordinates coords = (Coordinates)obj;
        return (Double.compare(this.Latitude, coords.Latitude) == 0
                && Double.compare(this.Longitude, coords.Longitude) == 0);
    }

    @Override
    public int hashCode() {
        return Objects.hash(this.Latitude, this.Longitude);
    }

}
